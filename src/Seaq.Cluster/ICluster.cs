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

namespace Seaq.Clusters{
    public interface ICluster
    {
        string ScopeId { get; }
        IEnumerable<ICollection> Collections { get; }

        IQueryResults<T> Query<T>(IQuery<T> criteria) 
            where T : class, IDocument;
        Task<IQueryResults<T>> QueryAsync<T>(IQuery<T> criteria) 
            where T : class, IDocument;
    }

    public interface ISqeeElasticsearchSerializer :
        Elasticsearch.Net.IElasticsearchSerializer
    {
        T Deserialize<T>(object data);
    }

    public class DefaultSqeeElasticsearchSerializer :
        ISqeeElasticsearchSerializer
    {
        readonly Func<string, Type, bool> TryGetCollectionType;

        public DefaultSqeeElasticsearchSerializer(
            Func<string, Type, bool> tryGetCollectionType)
        {
            TryGetCollectionType = tryGetCollectionType;
        }

        public object Deserialize(Type type, Stream stream)
        {
            return JsonSerializer.NonGeneric.Deserialize(type, stream, Utf8Json.Resolvers.StandardResolver.CamelCase);
        }

        public T Deserialize<T>(Stream stream)
        {
            return JsonSerializer.Deserialize<T>(stream, Utf8Json.Resolvers.StandardResolver.CamelCase);
        }

        public T Deserialize<T>(object data)
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.NonGeneric.Serialize(data), Utf8Json.Resolvers.StandardResolver.CamelCase);
        }

        public Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default)
        {
            return JsonSerializer.NonGeneric.DeserializeAsync(type, stream, Utf8Json.Resolvers.StandardResolver.CamelCase);
        }

        public Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
        {
            return JsonSerializer.DeserializeAsync<T>(stream, Utf8Json.Resolvers.StandardResolver.CamelCase);
        }

        public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None)
        {
            JsonSerializer.Serialize<T>(stream, data, Utf8Json.Resolvers.StandardResolver.CamelCase);
        }

        public Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None, CancellationToken cancellationToken = default)
        {
            return JsonSerializer.SerializeAsync<T>(stream, data, Utf8Json.Resolvers.StandardResolver.CamelCase);
        }
    }

    public static class FieldNameUtilities
    {
        public static string GetElasticPropertyName(
            Type type,
            string propertyName)
        {
            return GetElasticPropertyName(
                type,
                propertyName,
                Constants.Fields.KeywordField);
        }

        public static string GetElasticSortPropertyName(
            Type type,
            string propertyName)
        {
            return GetElasticPropertyName(
                type,
                propertyName,
                Constants.Fields.SortField);
        }

        public static string GetElasticPropertyNameWithoutSuffix(
            Type type,
            string propertyName)
        {
            return GetElasticPropertyName(type, propertyName, null);
        }

        public static string RemoveKnownPropertySuffixesFromPropertyName(
            string propertyName)
        {
            if (propertyName.EndsWith(Constants.Fields.KeywordField))
            {
                return propertyName.Substring(0, propertyName.Length - (Constants.Fields.KeywordField.Length + 1));
            }
            if (propertyName.EndsWith(Constants.Fields.SortField))
            {
                return propertyName.Substring(0, propertyName.Length - (Constants.Fields.SortField.Length + 1));
            }
            return propertyName;
        }

        public static Type GetSearchableType(
            string typeFullName)
        {
            var type =
                GetAllSearchableTypes()
                    .FirstOrDefault(x =>
                        x.FullName.Equals(typeFullName, StringComparison.OrdinalIgnoreCase));

            return type;
        }
        public static IEnumerable<Type> GetAllSearchableTypes()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var implements =
                allAssemblies
                    .SelectMany(p =>
                    {
                        try
                        {
                            return p.GetTypes();
                        }
                        catch (ReflectionTypeLoadException e)
                        {
                            return e.Types.Where(x => x != null);
                        }
                    })
                    .Where(p => typeof(IDocument).IsAssignableFrom(p))
                    .Select(p => p.Assembly);

            var types =
                implements
                    .SelectMany(x => x.GetTypes())
                    .Where(x => typeof(IDocument).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Distinct()
                    .ToList();

            return types;
        }
        private static string GetElasticPropertyName(
            Type type,
            string propertyName,
            string suffix)
        {
            var property = GetPropertyForType(type, propertyName);

            var chunks = propertyName.Split('.');
            var fieldName = "";
            foreach (var chunk in chunks)
            {
                fieldName = String.Join(".", fieldName, chunk);
            }

            if ((property.PropertyType == typeof(string) || property.PropertyType.GetGenericArguments()?.FirstOrDefault() == typeof(string)) && !String.IsNullOrWhiteSpace(suffix))
            {
                fieldName = $"{fieldName}.{suffix}";
            }

            return fieldName.Trim('.');
        }
        private static PropertyInfo GetPropertyForType(
            Type type,
            string propertyName)
        {
            if (type.BaseType == typeof(Array))
            {
                type = type.GetElementType();
            }

            var nameChunks = propertyName.Split('.').ToList();
            var propertyList = type.GetProperties();
            var interfacePropertyList = type.GetInterfaces().SelectMany(p => p.GetProperties());
            var props = propertyList.Concat(interfacePropertyList);

            var property = props.FirstOrDefault(p => p.Name.Equals(nameChunks.FirstOrDefault(), StringComparison.OrdinalIgnoreCase));
            nameChunks.RemoveAt(0);

            if (nameChunks.Count > 0)
                if (property.PropertyType.UnderlyingSystemType.GetProperties().Any())
                {
                    property = GetPropertyForType(property.PropertyType.UnderlyingSystemType, String.Join(".", nameChunks));
                }

            return property;
        }
    }

    public class Cluster :
        ICluster
    {
        public string ScopeId { get; }
        public IEnumerable<ICollection> Collections => _collections?.Values;

        private const bool _deleteOnServerDefault = false;
        private const bool _createCollectionForNewDocumentType = true;
        
        private Dictionary<string, ICollection> _collections;
        private ILookup<string, ICollection> _collectionsByType => _collections.Values.ToLookup(x => x.DocumentType.FullName, x => x);
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

        public bool? TryAddCollection(ICollectionConfig config, out ICollection collection)
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
                var alias = _client.Indices.PutAlias(Indices.Index(c.CollectionName), config.DocumentType.FullName);

                Log.Debug("Collection {0} successfully created.", c.CollectionName);

                c.SetSchema(new CollectionSchema(config));

                _collections.Add(c.CollectionName, c);
            }



            collection = c;
            return collection != default;
        }

        public bool? TryDeleteCollection(string collectionName, bool deleteOnServer = _deleteOnServerDefault)
        {
            if (!_collections.ContainsKey(collectionName))
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
            ICollection collection;
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
            ICollection collection;
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
        public bool? TryUpdateCollectionSchema(string collectionName, ICollectionSchema schema, out IEnumerable<string> messages)
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

        private Dictionary<string, ICollection> BuildCollectionCache(string scopeId, ElasticClient client, bool forceRefresh = false)
        {
            Log.Debug("Building collection cache for scope {0}", scopeId);
            var res = new Dictionary<string, ICollection>();

            var collectionNames = GetScopedCollectionList();

            var cached = Collections?.Where(x => collectionNames.Any(z => z.Equals(x.CollectionName, StringComparison.OrdinalIgnoreCase))) ?? Array.Empty<ICollection>();
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

    public class DocumentOperationError
    {
        public string CollectionName { get; set; }

        public string DocumentId { get; set; }
        public string Error { get; set; }

        public DocumentOperationError(
            string collectionName,
            string error,
            string documentId)
        {
            CollectionName = collectionName;
            Error = error;
            DocumentId = documentId;
        }
    }

    public static class ClusterExtensions
    {
        public static ConnectionSettings GetConnectionSettings(this Cluster cluster, IClusterConnection connection)
        {
            var url = connection.ClusterUrl;
            var username = connection.Username;
            var password = connection.Password;
            var serializer = connection.Serializer;

            var uri = new Uri(url);

            var connectionPool = new SingleNodeConnectionPool(uri);

            var connectionSettings =
                serializer == null ?
                new ConnectionSettings(connectionPool) :
                new ConnectionSettings(
                    connectionPool,
                    sourceSerializer: (builtin, settings) => serializer
                    );

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                connectionSettings.BasicAuthentication(username, password);
                ///this is the great big hammer to break out when things aren't working - 
                ///it bypasses certificate validation entirely, which is necessary for local self-signed certs,
                ///but is a GIANT security risk otherwise.  Only used for debugging for a reason.
#if DEBUG
                Log.Debug("DEBUG mode enabled.  Ignoring server certificate validation and enabling additional Elasticsearch debug messages.");
                connectionSettings.ServerCertificateValidationCallback((a, b, c, d) => true);
                connectionSettings.EnableDebugMode();
#endif
            }
            return connectionSettings;
        }    
            
    }

}
