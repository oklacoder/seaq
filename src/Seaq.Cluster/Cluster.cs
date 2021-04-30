using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using Serilog;
using Seaq;
using Utf8Json;

namespace Seaq.Clusters
{
    public class Cluster 
    {
        public string ScopeId { get; }
        public IEnumerable<Collection> Collections => _collections?.Values;

        private const bool _deleteOnServerDefault = false;
        private const bool _createCollectionForNewDocumentType = true;
        
        private Dictionary<string, Collection> _collections;
        private ILookup<string, Collection> _collectionsByType => _collections.Values.ToLookup(x => x.DocumentType.FullName, x => x);
        private ElasticClient _client = null;
        private ISqeeElasticsearchSerializer _serializer;
        private Dictionary<string, Type> _searchableTypes;

        public Cluster(
            ClusterConfig config)
        {
            Log.Debug("Building cluster for scope {0} at url {1}", config.ScopeId, config.Connection.ClusterUrl);

            ScopeId = config.ScopeId;

            if (_client == null)
            {
                Log.Debug("Building client for {0}@{1}", config.Connection.Username, config.Connection.ClusterUrl);
                var connectionSettings = this.GetConnectionSettings(config.Connection);
                _client = new ElasticClient(connectionSettings);
            }

            _serializer = config?.Connection.Serializer ?? new DefaultSqeeElasticsearchSerializer((x, y) => TryGetSearchType(x, out y));
            _collections = BuildCollectionCache(ScopeId, _client);
            _searchableTypes = FieldNameUtilities.GetAllSearchableTypes().ToDictionary(t => t.FullName, t => t);
        }

        public bool CanPing()
        {
            return _client.Ping().IsValid;
        }

        public bool? TryAddCollection(CollectionConfig config, out Collection collection)
        {
            collection = default;

            Log.Information("Creating collection {0} on cluster {1}", config.Name, this.ScopeId);
            Log.Debug("Settings: Type: {0}, Primary Shards: {1}, Replica Shards: {2}", config.DocumentType.FullName, config.PrimaryShards, config.ReplicaShards);

            if (config == null)
            {
                throw new ArgumentNullException($"Required parameter {nameof(config)} is null.");
            }
            
            if (_collections.ContainsKey(config.Name))
            {
                Log.Warning("A collection with the provided name {0} already exists on cluster {1}", config.Name, this.ScopeId);
                collection = _collections[config.Name];
                return null;
            }

            if (config.Name != config.Name.ToLowerInvariant())
            {
                Log.Error("Provided name ({0}) did not meet the requirement that all names be lowercase.", config.Name);
                throw new ArgumentException($"Provided name ({config.Name}) did not meet the requirement that all names be lowercase.", nameof(config.Name));
            }

            if (!config.Name.StartsWith(ScopeId))
            {
                Log.Information("Provided name ({0}) did not begin with cluster's scopeid ({1}).  Adjusting name to correct.", config.Name, this.ScopeId);
                config.AddScopeToName(ScopeId);
            }

            var c = new Collection(config);

            var createResult = _client.Indices.Create(config.Name, descriptor => descriptor.Extend(config, config.DocumentType));


            if (createResult.IsValid)
            {
                _collections.Add(c.CollectionName, c);
                var alias = _client.Indices.PutAlias(Indices.Index(c.CollectionName), config.DocumentType.FullName);

                Log.Debug("Collection {0} successfully created.", c.CollectionName);

                if (c.EagerlyPersistSchema)
                {
                    CollectionSchema schema;

                    var index = _client.Indices.Get(Indices.Index(c.CollectionName))?.Indices?.FirstOrDefault();

                    if (index.HasValue)
                    {
                        schema = new CollectionSchema(index.Value, config);
                        TryUpdateCollectionSchema(c.CollectionName, schema, out _);
                    }
                    else
                    {
                        schema = new CollectionSchema(config);
                    }
                    c.SetSchema(schema);
                    _collections[c.CollectionName] = c;
                }
            }



            collection = c;
            return collection != default;
        }

        public bool? TryDeleteCollection(string collectionName, bool deleteOnServer = _deleteOnServerDefault)
        {
            if (!_collections.ContainsKey(collectionName) && deleteOnServer != true)
            {
                Log.Error("Collection {0} not found on cluster {1}", collectionName, this.ScopeId);
                throw new KeyNotFoundException($"Collection {collectionName} not found on cluster {this.ScopeId}.");
            }

            if (!deleteOnServer)
            {
                _collections.Remove(collectionName);
                return true;
            }
            else
            {
                var deleteResponse = _client.Indices.Delete(Indices.Index(collectionName));

                if (!deleteResponse.IsValid)
                {
                    throw deleteResponse.OriginalException;
                }
                return deleteResponse.IsValid;                
            }
        }

        public bool? TryCommit<T>(T document)
            where T : class, IDocument
        {
            Collection collection;
            if (!_collections.ContainsKey(document.CollectionId))
            {
                Log.Error("Collection not found for id {0} and call not instructed to create it.", document.CollectionId);
                throw new KeyNotFoundException($"Collection not found for id {document.CollectionId} and call not instructed to create it.");
            }
            else
            {
                collection = _collections[document.CollectionId];
            }
            Log.Debug("Indexing document {0} to {1}", document.Id, collection.CollectionName);
            var result = _client.Index<T>(document, x => x.Index(collection.CollectionName).Id(document.Id).Refresh(collection.ForceRefreshOnDocumentCommit ? Refresh.True : Refresh.False));

            if (!result.IsValid)
            {
                Log.Error("Error in index attempt: {0}", result.OriginalException.Message);
                throw result.OriginalException;
            }

            return result.IsValid;
        }
        public bool? TryCommit<T>(ICollection<T> documents, out IEnumerable<DocumentOperationError> itemsWithErrors)
            where T : class, IDocument
        {
            Collection collection;
            var bulk = new BulkDescriptor();
            bool forceRefresh = false;

            foreach (var doc in documents)
            {
                if (!_collections.ContainsKey(doc.CollectionId))
                {
                    Log.Error("Collection not found for id {0}.", doc.CollectionId);
                    throw new KeyNotFoundException($"Collection not found for id {doc.CollectionId}.");
                }
                else
                {
                    collection = _collections[doc.CollectionId];
                }

                bulk.Index<T>(x => x.Index(doc.CollectionId).Id(doc.Id).Document(doc));

                if (collection.ForceRefreshOnDocumentCommit)
                    forceRefresh = true;

                //var result = _client.Index<T>(doc, x => x.Index(collection.CollectionName).Id(doc.Id).Refresh(collection.ForceRefreshOnDocumentCommit ? Refresh.True : Refresh.False));
            }
            bulk.Refresh(forceRefresh ? Refresh.True : Refresh.False);

            Log.Debug("Attempting to index {0} documents.", documents.Count());

            var result = _client.Bulk(bulk);

            itemsWithErrors = result.ItemsWithErrors.Select(x => new DocumentOperationError(x.Index, x.Error.Reason, x.Id));
            Log.Debug("Index attempt complete with {0} errors.", itemsWithErrors.Count());

            if (!result.IsValid)
            {
                Log.Error("Error in index attempt: {0}", result.OriginalException.Message);
                throw result.OriginalException;
            }

            return result.IsValid;
        }

        public bool? TryDelete<T>(string collectionName, out IEnumerable<DocumentOperationError> itemsWithErrors, params string[] ids)
            where T : class, IDocument
        {
            if (!_collections.ContainsKey(collectionName))
            {
                Log.Error("Collection not found for id {0}.", collectionName);
                throw new KeyNotFoundException($"Collection not found for id {collectionName}.");
            }

            var bulk = new BulkDescriptor().Refresh(
                _collections[collectionName].ForceRefreshOnDocumentCommit ? 
                Refresh.True : 
                Refresh.False);

            foreach(var id in ids)
            {
                bulk.Delete<T>(x => x.Index(collectionName).Id(id));
            }

            var result = _client.Bulk(bulk);

            itemsWithErrors = result.ItemsWithErrors.Select(x => new DocumentOperationError(x.Index, x.Error.Reason, x.Id));
            Log.Debug("Index attempt complete with {0} errors.", itemsWithErrors.Count());

            if (!result.IsValid)
            {
                Log.Error("Error in delete attempt: {0}", result.OriginalException.Message);
                throw result.OriginalException;
            }

            return result.IsValid;
        }
        public bool? TryUpdateCollectionSchema(string collectionName, CollectionSchema schema, out IEnumerable<string> messages)
        {
            messages = Array.Empty<string>();
            if (!_collections.ContainsKey(collectionName))
            {
                Log.Error("Could not find collection with name {0} on cluster {1}.", collectionName, this.ScopeId);
                throw new KeyNotFoundException($"Could not find collection with name {collectionName} on cluster {this.ScopeId}.");
            }

            var meta = new Dictionary<string, object>();
            meta.Add(Constants.CollectionSettings.SchemaKey, schema);
            var request = new PutMappingRequest(collectionName);
            request.Meta = meta;

            var putResult = _client.Indices.PutMapping(request);

            if (!putResult.IsValid)
            {
                Log.Error("Unable to update schema for {0} with error {1}", collectionName, putResult.OriginalException.Message);
                throw putResult.OriginalException;
            }
            else
            {
                Log.Information("Updated schema for collection {0}", collectionName);
                _collections[collectionName].SetSchema(schema);
            }

            return putResult.IsValid;
        }


        public IQueryResults<T> Query<T>(IQuery<T> query)
            where T : class, IDocument
        {
            Log.Debug("Executing sync query against cluster {0}, indices {1}", ScopeId, string.Join(", ", query.Criteria.Indices));
            return query.Execute(_client);
        }

        public async Task<IQueryResults<T>> QueryAsync<T>(IQuery<T> query)
            where T : class, IDocument
        {
            Log.Debug("Executing async query against cluster {0}, indices {1}", ScopeId, string.Join(", ", query.Criteria.Indices));
            return await query.ExecuteAsync(_client);
        }

        
        public IEnumerable<string> GetScopedCollectionList()
        {
            Log.Debug("Fetching scoped collection list for scope {0}", ScopeId);
            var query = new GetIndexRequest(Indices.Index($"{ScopeId}*"));
            var collections =
                _client.Indices
                    .Get(query)?
                        .Indices?
                            .Keys?
                                .Select(x => x.Name);

            return collections;
        }

        private Dictionary<string, Collection> BuildCollectionCache(string scopeId, ElasticClient client, bool forceRefresh = false)
        {
            Log.Debug("Building collection cache for scope {0}", scopeId);
            var res = new Dictionary<string, Collection>();

            var collectionNames = GetScopedCollectionList();

            var cached = Collections?.Where(x => collectionNames.Any(z => z.Equals(x.CollectionName, StringComparison.OrdinalIgnoreCase))) ?? Array.Empty<Collection>();
            var toFetch = Collections == null ?
                collectionNames :
                Collections.Where(x => !collectionNames.Any(z => z.Equals(x.CollectionName, StringComparison.OrdinalIgnoreCase))).Select(x => x.CollectionName);

            if (forceRefresh)
            {
                toFetch = toFetch.Concat(cached.Select(x => x.CollectionName));
            }
            else
            {
                foreach (var c in cached)
                {
                    res.Add(c.CollectionName, c);
                }
            }

            if (toFetch?.Any() == true)
            {
                var indices = _client.Indices
                    .Get(Indices.Index(toFetch));

                var schemas = indices
                    .Indices
                    .Select(x => x.Value?.Mappings?.Meta)
                    .Select(BuildSchemaFromMeta);

                foreach(var s in schemas)
                {
                    if (s != null)
                    {
                        var q = new Collection(s);
                        res.Add(q.CollectionName, q);
                    }
                }
            }

            return res;
        }
        private CollectionSchema BuildSchemaFromMeta(IDictionary<string, object> meta)
        {
            if (meta == null || !meta.ContainsKey(Constants.CollectionSettings.SchemaKey))
                return null;

            var raw = meta[Constants.CollectionSettings.SchemaKey];
            return _serializer.Deserialize<CollectionSchema>(raw);
        }
        private bool TryGetSearchType(
            string typeFullName,
            out Type type)
        {
            return _searchableTypes.TryGetValue(typeFullName, out type);
        }
    }

}
