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
            Log.Debug("Building Cluster for scope {0} at url {1}", settings.ScopeId, settings.Url);

            _serializer = settings.Serializer ??
                        new NewtonsoftElasticsearchSerializer((x) => TryGetSearchType(x));
            //new MessagePackElasticsearchSerializer((x) => TryGetStoreType(x));

            Log.Debug("Building client for {0}@{1}", settings.Username, settings.Url);

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
                Log.Debug("DEBUG mode enabled.  Ignoring server certificate validation and enabling additional Elasticsearch debug messages.");
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

            var storeId = new StoreId(schema.StoreId);
            store = new Store(
                storeId, 
                schema, 
                schema.Type);

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

            Log.Information("Creating store {0}", StoreId.FormatAsIndexId(settings.ScopeId, settings.Moniker));
            Log.Debug("Additional settings: TypeName: {0}, Primary Shards: {1}, Replica Shards: {2}", settings.TypeFullName, settings.PrimaryShards, settings.ReplicaShards);

            Store store = null;
            if (settings == null)
            {
                throw new ArgumentNullException($"Parameter {nameof(settings)} is null or invalid.");
            }
            var type = _fieldNameUtilities.GetSearchableType(settings.TypeFullName);
            if (type == null)
            {
                Log.Error("Provided schema type {0} is not present on the cluster.", settings.TypeFullName);
                Log.Warning("Ensure that you have correctly imported the relevant namespace for type {0}", settings.TypeFullName);
                return null;
            }

            store = new Store(settings);

            if (_stores.ContainsKey(store.StoreId.Name))
            {
                Log.Warning($"Creation of store {store.StoreId.Name} failed - a store with that name already exists.");
                return _stores[store.StoreId.Name];
            }

            var createResult =
            _client.Indices.Create(
                store.StoreId.Name,
                descriptor => descriptor.Extend(settings, type));

            var alias = _client.Indices.PutAlias(Indices.Index(store.StoreId.Name), settings.TypeFullName);

            if (createResult.IsValid)
            {
                Log.Debug("Store successfully created.");
                var schema = new StoreSchema(settings);
                store = new Store(store, schema);
                _stores.Add(store.StoreId.Name, store);

                if (Props.EagerlyPersistStoreMetaDefault || settings.EagerlyPersistStoreMeta)
                {
                    Log.Debug("Persisting store metadata...");
                    schema = BuildStoreSchema(createResult.Index);
                    var saveSchemaResult = SaveStoreSchema(createResult.Index, schema);
                    if (saveSchemaResult)
                    {
                        Log.Debug("Metadata persist successful.");
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
            Log.Debug("Executing method DeleteStore");
            Log.Information("Attempting to delete store {0}", storeIdName);

            bool success = false;
            if (string.IsNullOrWhiteSpace(storeIdName))
            {
                throw new ArgumentNullException($"Parameter {nameof(storeIdName)} is null or invalid.");
            }


            if (!DoesDataStoreExist(storeIdName))
            {
                Log.Information("Store with name {0} not found.", storeIdName);
                return success;
            }

            var deleteResult = _client.Indices.Delete(storeIdName);

            if (deleteResult.IsValid)
            {
                Log.Information("Store {0} successfully deleted.", storeIdName);
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
            Log.Debug("Attempting to persist {0} documents.", documents.Length);

            if (!documents.Any())
            {
                throw new ArgumentNullException($"Parameter {nameof(documents)} is null or empty.");
            }

            return _Commit(documents, Props.ForceRefreshOnCommit);
        }

        public string[] Commit(
            IDocument[] documents,
            bool overrideForceRefreshOnCommitSetting)
        {
            Log.Debug("Attempting to persist {0} documents with forceRefreshOverride of {1}", documents.Length, overrideForceRefreshOnCommitSetting);
            if (!documents.Any())
            {
                throw new ArgumentNullException($"Parameter {nameof(documents)} is null or empty.");
            }

            return _Commit(documents, overrideForceRefreshOnCommitSetting);
        }

        private string[] _Commit(
            IDocument[] documents,
            bool overrideForceRefreshOnCommitSetting)
        {

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

            Log.Debug("Document persist complete with {0} errors.", bulkResult.ItemsWithErrors.Count());

            return bulkResult.ItemsWithErrors.Select(x => $"{x.Id} - {x.Error?.Reason}").ToArray();
        }

        public string[] DeleteDocuments(
            IDocument[] documents)
        {
            Log.Debug("Attempting to delete {0} documents.", documents.Length);
            if (!documents.Any())
            {
                throw new ArgumentNullException($"Parameter {nameof(documents)} is null or empty.");
            }
            return _DeleteDocuments(documents, Props.ForceRefreshOnCommit);
        }
        public string[] DeleteDocuments(
            IDocument[] documents,
            bool overrideForceRefreshOnCommitSetting)
        {
            Log.Debug("Attempting to delete {0} documents with forceRefreshOverride of {1}", documents.Length, overrideForceRefreshOnCommitSetting);
            if (!documents.Any())
            {
                throw new ArgumentNullException($"Parameter {nameof(documents)} is null or empty.");
            }
            return _DeleteDocuments(documents, overrideForceRefreshOnCommitSetting);
        }

        private string[] _DeleteDocuments(
            IDocument[] documents,
            bool overrideForceRefreshOnCommitSetting)
        {

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

            Log.Debug("Document delete complete with {0} errors.", bulkResult.ItemsWithErrors.Count());

            return bulkResult.ItemsWithErrors.Select(x => $"{x.Id} - {x.Error?.Reason}").ToArray();
        }

        public IQueryResult Query<TDocument>(
            IQuery<TDocument> query)
            where TDocument : class, IDocument
        {
            Log.Debug("Executing method Query.");
            Log.Debug("Query type: {0}", query.GetType()?.FullName);
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
            Log.Debug("Checking if store {0} exists.", storeIdName);
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
            Log.Debug("Generating schema for store {0}", storeIdName);
            var indices = _client.Indices.Get(Indices.Index(storeIdName));
            var index = indices.Indices.FirstOrDefault();

            var storeSchema = new StoreSchema(index);
            return storeSchema;
        }

        private StoreSchema[] BuildStoreSchemas(
            params string[] storeIdNames)
        {
            Log.Debug("Generating schema for {0} stores.", storeIdNames.Length);
            return storeIdNames.Select(x => BuildStoreSchema(x)).ToArray();
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

        private string TryGetStoreTypeFullName(
            string storeIdName)
        {
            if (_stores.ContainsKey(storeIdName))
                return _stores[storeIdName]?.TypeFullName;
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
            Log.Information("Attempting to save schema for store {0}", storeIdName);
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
                Log.Information("Schema saved succesfully.");
                if (_stores.ContainsKey(storeIdName))
                    _stores[storeIdName] = new Store(_stores[storeIdName], schema);
            }

            return putResult.IsValid;
        }
    }
}
