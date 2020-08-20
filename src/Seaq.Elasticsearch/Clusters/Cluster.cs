using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Elasticsearch.Net;
using Nest;
using Seaq.Elasticsearch.Documents;
using Seaq.Elasticsearch.Extensions;
using Seaq.Elasticsearch.Queries;
using Seaq.Elasticsearch.Stores;
using Serilog;

namespace Seaq.Elasticsearch.Clusters
{

    public class Cluster
    {
        private readonly ElasticClient _client;
        public readonly ClusterProps Props;
        public readonly Dictionary<string, Type> SearchTypes;
        private readonly IFieldNameUtilities _fieldNameUtilities;
        private readonly IDocumentPropertyBuilder _documentPropertyBuilder;
        private readonly IResultsBuilder _resultsBuilder;
        private readonly ISeaqElasticsearchSerializer _serializer;

        private Dictionary<string, Store> _stores { get; }
        public IEnumerable<Store> Stores => _stores?.Values;

        

        public Cluster(
            ClusterSettings settings,
            IFieldNameUtilities fieldNameUtilities = null,
            IDocumentPropertyBuilder documentPropertyBuilder = null,
            IResultsBuilder resultsBuilder = null)
        {
            _serializer = settings.Serializer ??
                        new NewtonsoftElasticsearchSerializer((x) => TryGetSearchType(x));
                        //new MessagePackElasticsearchSerializer((x) => TryGetStoreType(x));

            _client = GetClient(
                GetConnectionSettings(
                    settings.Url, 
                    settings.Username, 
                    settings.Password,
                    _serializer
                    ));


            Props = new ClusterProps(
                settings.ScopeId,
                settings.ForceRefreshOnCommit,
                settings.EagerlyCreateIndexMetadataRecord);


            _fieldNameUtilities = fieldNameUtilities ?? new DefaultFieldNameUtilities();
            _documentPropertyBuilder = documentPropertyBuilder ?? new DefaultDocumentPropertyBuilder();
            _resultsBuilder = resultsBuilder ?? new DefaultResultsBuilder(_fieldNameUtilities, _documentPropertyBuilder);
            SearchTypes = _fieldNameUtilities.GetAllSearchableTypes().ToDictionary(t => t.FullName, t => t);
            _stores = FillStoreList(); //new Dictionary<string, Store>();
        }

        private ConnectionSettings GetConnectionSettings(
            string url,
            string username,
            string password,
            ISeaqElasticsearchSerializer serializer)
        {
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
                connectionSettings.ServerCertificateValidationCallback((a, b, c, d) => true);
                connectionSettings.EnableDebugMode();
#endif
            }
            return connectionSettings;
        }

        private ElasticClient GetClient(
            ConnectionSettings settings)
        {
            return new ElasticClient(settings);
        }

        private Store BuildStoreFromSchema(
            StoreSchema schema)
        {
            Log.Debug("Executing method BuildStoreFromSchema.");
            Log.Debug("Parameters: {0}:{1}", nameof(schema), schema);
            Store store;

            if (schema == null) return null;

            if(!SearchTypes.ContainsKey(schema.StoreType)){
                Log.Error("Provided schema type {0} is not present on the cluster.", schema.StoreType);
                Log.Warning("Ensure that you have correctly imported the relevant namespace for type {0}", schema.StoreType);
                return null;
            }

            var type = SearchTypes[schema.StoreType];
            var storeId = new StoreId(schema.StoreId);
            store = new Store(
                storeId, 
                schema, 
                type);

            return store;
        }

        private Dictionary<string, Store> FillStoreList()
        {
            Log.Debug("Executing method FillStoreList");

            var stores = GetScopedStoreList();
            if (!stores.Any())
            {
                return new Dictionary<string, Store>();
            }
            var schemas = GetStoreSchemas(stores);
            return schemas
              .Select(BuildStoreFromSchema)
              .Where(x => x != null)
              .ToDictionary(x => x.StoreId.Name, x => x);
        }
        public bool CanPing()
        {
            return _client.Ping().IsValid;
        }

        public string[] GetScopedStoreList()
        {
            Log.Debug("Executing method GetScopedStoreList");

            var query = new GetIndicesQuery($"{Props.ScopeId}*");
            var stores =
                _client.Indices
                    .Get(query.Request)?
                        .Indices?
                            .Keys?
                                .Select(x => x.Name)?
                                    .ToArray();

            Log.Information("{0} stores found.", stores.Length);
            Log.Debug("Found stores: {0}", stores);
            return stores;
        }
        public Store CreateStore(
            CreateStoreSettings settings)
        {
            Store store = null;
            if (settings == null)
            {
                throw new ArgumentNullException($"Parameter {nameof(settings)} is null or invalid.");
            }

            store = new Store(settings);
            var createResult =
                _client.Indices.Create(
                    store.StoreId.Name,
                    descriptor => descriptor.Extend(settings));

            var alias = _client.Indices.PutAlias(Indices.Index(store.StoreId.Name), settings.Type.FullName);

            if (createResult.IsValid)
            {
                var schema = new StoreSchema(settings);
                store = new Store(store, schema);
                _stores.Add(store.StoreId.Name, store);

                if (Props.EagerlyPersistStoreMetaDefault || settings.EagerlyPersistStoreMeta)
                {
                    schema = BuildStoreSchema(createResult.Index);
                    var saveSchemaResult = SaveStoreSchema(createResult.Index, schema);
                    if (saveSchemaResult)
                    {
                        store = new Store(store, schema);
                    }
                }
            }
            else
            {
                //TODO: something
                return null;
            }

            return store;
        }

        public bool DeleteStore(
            string storeIdName)
        {
            bool success = false;
            if (string.IsNullOrWhiteSpace(storeIdName))
            {
                throw new ArgumentNullException($"Parameter {nameof(storeIdName)} is null or invalid.");
            }


            if (!DoesDataStoreExist(storeIdName))
            {
                return success;
            }

            var deleteResult = _client.Indices.Delete(storeIdName);

            if (deleteResult.IsValid)
            {
                //_client.Index.
                _stores.Remove(storeIdName);
                    
                success = true;
            }
            else
            {
                return success;
            }

            return success;
        }
        
        public string[] Commit(
            IDocument[] documents)
        {
            if (!documents.Any())
            {
                throw new ArgumentNullException($"Parameter {nameof(documents)} is null or empty.");
            }
            return Commit(documents, Props.ForceRefreshOnCommit);
        }

        public string[] Commit(
            IDocument[] documents,
            bool overrideForceRefreshOnCommitSetting)
        {
            if (!documents.Any())
            {
                throw new ArgumentNullException($"Parameter {nameof(documents)} is null or empty.");
            }

            var bulkDescriptor = new BulkDescriptor();

            if (overrideForceRefreshOnCommitSetting == true)
            {
                bulkDescriptor.Refresh(Refresh.True);
            }

            foreach (var document in documents)
            {
                bulkDescriptor.Index<object>(d => d.Index(document.StoreId).Document(document).Id(document.DocumentId));
            }

            var bulkResult = _client.Bulk(bulkDescriptor);

            return bulkResult.ItemsWithErrors.Select(x => $"{x.Id} - {x.Error?.Reason}").ToArray();
        }
        public string[] DeleteDocuments(
            IDocument[] documents)
        {
            if (!documents.Any())
            {
                throw new ArgumentNullException($"Parameter {nameof(documents)} is null or empty.");
            }
            return DeleteDocuments(documents, Props.ForceRefreshOnCommit);
        }
        public string[] DeleteDocuments(
            IDocument[] documents,
            bool overrideForceRefreshOnCommitSetting)
        {
            if (!documents.Any())
            {
                throw new ArgumentNullException($"Parameter {nameof(documents)} is null or empty.");
            }

            var bulkDescriptor = new BulkDescriptor();

            if (overrideForceRefreshOnCommitSetting == true)
            {
                bulkDescriptor.Refresh(Refresh.True);
            }

            foreach (var document in documents)
            {
                bulkDescriptor.Delete<object>(d => d.Index(document.StoreId).Document(document).Id(document.DocumentId));
            }

            var bulkResult = _client.Bulk(bulkDescriptor);

            return bulkResult.ItemsWithErrors.Select(x => x.Id).ToArray();
        }

        public IQueryResult Query<TDocument>(
            IQuery<TDocument> query)
            where TDocument : class, IDocument
        {
            Log.Debug("Executing method Query.");
            Log.Debug("Parameters: {0}:{1}", nameof(query), query);
            if (query == null)
            {
                throw new ArgumentNullException($"Parameter {nameof(query)} is null or invalid.");
            }

            query.Criteria.CollectMetadataForQuery(this);

            var searchResults = _client.Search(query.Criteria.GetDescriptor());
            
            return _resultsBuilder.GetQueryResultByCriteriaType(query.Criteria, searchResults);
        }
        
        public bool DoesDataStoreExist(
            string storeIdName)
        {
            if (string.IsNullOrWhiteSpace(storeIdName))
            {
                throw new ArgumentNullException($"Parameter {nameof(storeIdName)} is null or invalid.");
            }

            return Stores.Any(p => p.StoreId.Name == storeIdName);
        }

        public StoreSchema GetStoreSchema(
            string storeIdName)
        {
            Log.Debug("Executing method GetStoreSchema.");
            Log.Debug("Parameters: {0}:{1}", nameof(storeIdName), storeIdName);
            if (string.IsNullOrWhiteSpace(storeIdName))
            {
                throw new ArgumentNullException($"Parameter {nameof(storeIdName)} is null or invalid.");
            }

            StoreSchema storeSchema;

            if (_stores.ContainsKey(storeIdName))
            {
                storeSchema = _stores[storeIdName].StoreSchema;
            }
            else
            {
                var indices = _client.Indices.Get(Indices.Index(storeIdName));
                var index = indices.Indices.FirstOrDefault();

                storeSchema = BuildSchemaFromMeta(index.Value?.Mappings?.Meta);
            }

            return storeSchema;
        }

        public StoreSchema[] GetStoreSchemas(
            params string[] storeIdNames)
        {
            Log.Debug("Executing method GetStoreSchemas.");
            Log.Debug("Parameters: {0}:{1}", nameof(storeIdNames), storeIdNames);

            if (!storeIdNames.Any())
            {
                throw new ArgumentNullException($"Parameter {nameof(storeIdNames)} is null or invalid.");
            }

            var schemas = new List<StoreSchema>();

            var cached = storeIdNames?
                .Where(p => _stores?.ContainsKey(p) == true)?
                .ToArray();
                

            var toFetch = _stores == null ?
                storeIdNames :
                storeIdNames?
                .Where(p => !_stores?.ContainsKey(p) == true)?
                .ToArray();

            if (cached != null && cached.Any())
            {
                var cachedSchemas = cached.Select(p => _stores[p].StoreSchema);
                schemas.AddRange(cachedSchemas);
            }

            if (toFetch != null && toFetch.Any())
            {
                var indices = _client.Indices
                    .Get(Indices.Index(toFetch));

                var fetchedSchemas = 
                    indices.Indices
                    .Select(x => x.Value?.Mappings?.Meta)
                    .Select(BuildSchemaFromMeta); 

                schemas.AddRange(fetchedSchemas);
            }

            return schemas.Where(x => x != null).ToArray();
        }

        private StoreSchema BuildStoreSchema(
            string storeIdName)
        {
            var indices = _client.Indices.Get(Indices.Index(storeIdName));
            var index = indices.Indices.FirstOrDefault();

            var storeSchema = new StoreSchema(index);
            return storeSchema;
        }

        private StoreSchema[] BuildStoreSchemas(
            params string[] storeIdNames)
        {

            var indices = _client.Indices.Get(Indices.Index(storeIdNames));
            var schemas = indices.Indices.Select(p => new StoreSchema(p));

            return schemas.ToArray();
        }
        
        private StoreSchema BuildSchemaFromMeta(
            IDictionary<string, object> meta)
        {
            if (meta == null || !meta.ContainsKey(WellKnownKeys.IndexSettings.StoreSchema))
                return null;
            var rawMeta = meta[WellKnownKeys.IndexSettings.StoreSchema];
            var returnValue = _serializer.Deserialize<StoreSchema>(rawMeta);
            return returnValue;
        }

        private Type TryGetStoreType(
            string storeIdName)
        {
            if (_stores.ContainsKey(storeIdName))
                return _stores[storeIdName]?.Type;
            else
                return null;
        }

        private Type TryGetSearchType(
            string typeFullName)
        {
            if (SearchTypes.ContainsKey(typeFullName))
                return SearchTypes[typeFullName];
            else
                return null;
        }

        public bool SaveStoreSchema(
            string storeIdName, 
            StoreSchema schema)
        {
            if (!_stores.ContainsKey(storeIdName))
            {
                throw new ArgumentNullException($"Parameter {nameof(storeIdName)} is null or invalid.");
            }
            if (schema == null)
            {
                throw new ArgumentNullException($"Parameter {nameof(schema)} is null or invalid.");
            }

            var meta = new Dictionary<string, object>();

            meta.Add(WellKnownKeys.IndexSettings.StoreSchema, schema);

            var request = new PutMappingRequest(storeIdName);
            request.Meta = meta;

            var putResult = _client.Indices.PutMapping(request);

            if (putResult.IsValid)
            {
                if (_stores.ContainsKey(storeIdName))
                    _stores[storeIdName] = new Store(_stores[storeIdName], schema);
            }

            return putResult.IsValid;
        }
    }
}
