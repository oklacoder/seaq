using Elasticsearch.Net;
using Nest;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seaq
{

    public static class ClusterExtensions
    {

    }
    public partial class Cluster
    {
        public event EventHandler IndexCacheInitializing;
        public event EventHandler IndexCacheInitialized;
        public event EventHandler IndexCacheRefreshed;
        public event EventHandler IndexCacheRefreshing;

        public string ClusterScope { get; set; }

        /// <summary>
        /// List of all cluster indices
        /// </summary>
        public IEnumerable<Index> Indices => _indices?.Values;
        /// <summary>
        /// Lookup of cluster indices, grouped by dotnet type
        /// </summary>
        public ILookup<string, Index> IndicesByType => _indices?.Values.ToLookup(x => x.DocumentType, x => x);
        /// <summary>
        /// Lookup of deprecated indices
        /// </summary>
        public IEnumerable<Index> DeprecatedIndices => _indices?.Values.Where(x => x.IsDeprecated is true);
        /// <summary>
        /// Lookup of *not* deprecated indices
        /// </summary>
        public IEnumerable<Index> NotDeprecatedIndices => _indices?.Values.Where(x => x.IsDeprecated is not true);
        /// <summary>
        /// Lookup of hidden indices
        /// </summary>
        public IEnumerable<Index> HiddenIndices => _indices?.Values.Where(x => x.IsHidden is true);
        /// <summary>
        /// Lookup of *not* hidden indices
        /// </summary>
        public IEnumerable<Index> NotHiddenIndices => _indices?.Values.Where(x => x.IsHidden is not true);
        /// <summary>
        /// Lookup of indices included in global results
        /// </summary>
        public IEnumerable<Index> GlobalResultIndices => _indices?.Values.Where(x => x.ReturnInGlobalSearch is true);
        /// <summary>
        /// Lookup of indices *not* included in global results
        /// </summary>
        public IEnumerable<Index> NotGlobalResultIndices => _indices?.Values.Where(x => x.ReturnInGlobalSearch is not true);
        
        /// <summary>
        /// List of loaded types that implement the BaseDocument interface
        /// </summary>
        public IEnumerable<Type> SearchableTypes => _searchableTypes.Values;

        public bool IsCacheInitializing { get; private set; } = false;
        public DateTime? CacheInitializedUtc { get; private set; } = null;
        public bool IsCacheRefreshing { get; private set; } = false;
        public DateTime? CacheRefreshedUtc { get; private set; } = null;

        public bool AllowAutomaticIndexCreation { get; private set; }
        public string InternalStoreIndex => $"{ClusterScope}_{IndexNameUtilities.FormatIndexName(typeof(Index))}";

        private Dictionary<string, Index> _indices;
        private Dictionary<string, Type> _searchableTypes;
        private ISeaqElasticsearchSerializer _serializer;
        private readonly ElasticClient _client;

        public static Cluster Create(
            ClusterArgs args)
        {
            var resp = new Cluster(args);

            resp.InitializeIndexCache().Wait();

            return resp;
        }

        public static async Task<Cluster> CreateAsync(
            ClusterArgs args)
        {
            var resp = new Cluster(args);

            await resp.InitializeIndexCache();

            return resp;
        }

        private Cluster(
            ClusterArgs args)
        {
            ClusterScope = args.ClusterScope;
            AllowAutomaticIndexCreation = args.AllowAutomaticIndexCreation;
            _serializer = args.Serializer ?? new DefaultSeaqElasticsearchSerializer((x) => TryGetSearchType(x));

            _client = BuildClient(args, _serializer);

            _searchableTypes = FieldNameUtilities.GetAllSearchableTypes().ToDictionary(t => t.FullName, t => t);

            ConfigureEventHandlers();
        }

        //can ping cluster
        public bool CanPing()
        {
            var resp = _client.Ping();
            return resp.IsValid;
        }
        public async Task<bool> CanPingAsync()
        {
            return (await _client.PingAsync()).IsValid;
        }

        //init index collection
        public async Task InitializeIndexCache()
        {
            IndexCacheInitializing?.Invoke(this, null);

            _indices = new Dictionary<string, Index>();

            var query = new GetIndexRequest(Nest.Indices.Index($"{ClusterScope}*"));
            var resp = await _client.Indices
                .GetAsync(query);

            var vals = resp.Indices.Select(Index.Create); 
            
            foreach (var v in vals)
                _indices[v.Name] = v;

            if (!IndicesByType.Contains(typeof(seaq.Index).FullName))
            {
                var indexConfig = new IndexConfig(
                    typeof(seaq.Index).FullName, typeof(Index).FullName);
                await CreateIndexAsync(indexConfig);
                await HydrateInternalStore();
            }
            await RefreshFromInternalStore();

            IndexCacheInitialized?.Invoke(this, null);
        }

        //refresh index collection
        public async Task RefreshIndexCache()
        {
            IndexCacheRefreshing?.Invoke(this, null);

            var query = new GetIndexRequest(Nest.Indices.Index($"{ClusterScope}*"));
            var resp = await _client.Indices
                .GetAsync(query);

            var vals = resp.Indices.Select(Index.Create);
            foreach (var v in vals)
                _indices[v.Name] = v;
            
            await RefreshFromInternalStore();

            await HydrateInternalStore();

            IndexCacheRefreshed?.Invoke(this, null);
        }

        //add index

        public Index CreateIndex(
            IndexConfig config)
        {
            return CreateIndexAsync(config).Result;
        }

        public async Task<Index> CreateIndexAsync(
            IndexConfig config)
        {
            Log.Debug("Attempting to create index {0}", config.Name);

            if (!config.Name.Equals(config.Name.ToLowerInvariant()))
            {
                var oldName = config.Name;
                var newName = IndexNameUtilities.FormatIndexName(config.Name);

                config.Name = newName;

                Log.Warning("Provided index name ({0}) was not all lowercase as required - index name coerced to {1}", oldName, newName);
            }

            if (!config.Name.StartsWith(ClusterScope, StringComparison.OrdinalIgnoreCase))
            {
                var oldName = config.Name;
                var newName = string.Join(Constants.Indices.NamePartSeparator, ClusterScope, config.Name);

                config.Name = newName;

                Log.Warning("Provided index name ({0}) did not begin with cluster's scope as required - index name coerced to {1}", oldName, newName);
            }

            if (_indices.ContainsKey(config.Name))
            {
                Log.Warning("Couldn't create index {0} - an index with that name already exists in cluster cache.", config.Name);
                await Task.CompletedTask;
                return _indices[config.Name];
            }

            if (!_searchableTypes.TryGetValue(config.DocumentType, out var type))
            {
                Log.Error("Attempted to create an index for type {0}, which is not present in the cluster's type cache.  Reference the type prior to attempting index creation to ensure that its assembly is forced to load.", config.DocumentType);
                await Task.CompletedTask;
                return null;
            }

            if (!string.IsNullOrWhiteSpace(config.IndexAsType))
            {
                var any = IndicesByType[config.IndexAsType];
                if (any?.Any() is not true)
                {
                    Log.Error("Cannot create an index with an {0} mapping of {1} since no index with type {1} exists on the cluster",
                        nameof(IndexConfig.IndexAsType), config.IndexAsType, config.IndexAsType);
                    return null;
                }
                var t0 = SearchableTypes.FirstOrDefault(x => x.FullName.Equals(config.IndexAsType, StringComparison.OrdinalIgnoreCase));
                var t = SearchableTypes.FirstOrDefault(x => x.FullName.Equals(config.DocumentType, StringComparison.OrdinalIgnoreCase));
                if (t0 == null)
                {
                    //can't
                    Log.Error($"Couldn't find specified {nameof(Index.IndexAsType)} of {t0.Name} across loaded assemblies.  " +
                        $"Make sure you've referenced it prior to initializing your seaq cluster to ensure that" +
                        $"the runtime has loaded it.");
                    return null;
                }
                if (t == null)
                {
                    //can't
                    Log.Error($"Couldn't find specified {nameof(Index.DocumentType)} of {t.Name} across loaded assemblies.  " +
                        $"Make sure you've referenced it prior to initializing your seaq cluster to ensure that" +
                        $"the runtime has loaded it.");
                    return null;
                }
                if (!t.IsSubclassOf(t0))
                {
                    //still can't
                    Log.Error($"When specifying {nameof(Index.IndexAsType)}, the provided {nameof(Index.DocumentType)} must be a subclass of the specified {nameof(Index.IndexAsType)}");
                    Log.Error($"{t.FullName} does not implement {t0.FullName} as required.");
                    return null;
                }

                //we do this dance  *specifically and explicitly* to avoid creating an index on the server,
                //because oversharding is bad mmmkay.
                //with that in mind, we short-circuit the creation process here.
                var idx = new Index(config);
                idx.Fields = any.First().Fields;
                _indices.Add(idx.Name, idx);
                await SaveToInternalStore(idx);
                return idx;
            }

            var result = await _client.Indices.CreateAsync(config.Name, desc => desc.Extend(config, type));

            if (!result.IsValid)
            {
                Log.Error("Could not successfully create requested index {0}", config.Name);
                Log.Error(result.ServerError?.Error?.Reason);
                Log.Error(result.OriginalException.Message);
                Log.Error(result.OriginalException.StackTrace);
                await Task.CompletedTask;
                return null;
            }

            Log.Debug("Index {0} created successfully", config.Name);

            var server_index = _client.Indices.Get(config.Name)?.Indices?.FirstOrDefault();

            if (!server_index.HasValue)
            {
                Log.Error("Index {0} creation reported success, but could not retrieve index definition from server.  This cannot be automatically resolved - suggest handling with index cache refresh.", config.Name);
                throw new InvalidOperationException(string.Format("Index {0} creation reported success, but could not retrieve index definition from server.  This cannot be automatically resolved - suggest handling with index cache refresh.", config.Name));
            }

            var index = Index.Create(server_index.Value);

            _indices.Add(index.Name, index);

            await SaveToInternalStore(index);

            return index;
        }

        //remove index

        public bool DeleteIndex(
            string indexName,
            bool bypassCache = false,
            bool preserveDependentIndices = false)
        {
            return DeleteIndexAsync(indexName, bypassCache).Result;
        }

        public async Task<bool> DeleteIndexAsync(
            string indexName,
            bool bypassCache = false,
            bool preserveDependentIndices = false)
        {
            Log.Debug("Deleting index {0}", indexName);

            if (!_indices.ContainsKey(indexName) && bypassCache is not true)
            {
                Log.Warning("Delete index failed - {0} is not present in cluster cache.", indexName);
                await Task.CompletedTask;
                return false;
            }
            
            var req = new DeleteIndexDescriptor(Nest.Indices.Index(indexName));

            var result = await _client.Indices.DeleteAsync(req);

            if (!result.IsValid)
            {
                Log.Error("Could not successfully delete index {0}", indexName);
                Log.Error(result.ServerError?.Error.Reason);
                Log.Error(result.OriginalException.Message);
                Log.Error(result.OriginalException.StackTrace);
                await Task.CompletedTask;
                return false;
            }

            var idx = _indices[indexName];
            if (preserveDependentIndices is not true)
            {
                var dependants = Indices.Where(x => x?.IndexAsType?.Equals(idx?.DocumentType) is true);
                foreach (var dependant in dependants)
                    _indices.Remove(dependant.Name);
            }
            if (_indices.Remove(indexName))
            {
                await DeleteFromInternalStore(idx);
            }
            else
            {
                const string msg = "Index {0} deletion reported success, but could not remove index from local cache.  This cannot be automatically resolved - suggest handling with index cache refresh.";
                Log.Error(msg, indexName);
                throw new InvalidOperationException(string.Format(msg, indexName));
            }


            return true;
        }

        //copy index - schema only/with docs

        public async Task<Index> CopyIndexAsync(
            string sourceIndex,
            string targetIndex,
            bool includeDocs = false)
        {
            Log.Debug("Creating index {0} as copy of {1}", targetIndex, sourceIndex);
            if (!_indices.ContainsKey(sourceIndex))
            {
                const string err = @"Could not find index {0} on cluster.";
                Log.Error(err, sourceIndex);
                return null;
            }

            var config = new IndexConfig(_indices[sourceIndex]);
            config.Name = targetIndex;
            var resp = await CreateIndexAsync(config);

            if (resp is null)
            {
                const string err = @"Could not create index due to unknown error.  Please try again.";
                Log.Error(err);
                return null;
            }

            if (includeDocs is true)
            {
                Log.Debug("Beginning reindex from {0} to {1}", sourceIndex, resp.Name);
                var docResp = _client.ReindexOnServer(x => x
                    .Source(z => 
                        z.Index(sourceIndex))
                    .Destination(z => 
                        z.Index(resp.Name))
                    .WaitForCompletion());
                Log.Debug("Completed reindex from {0} to {1}", sourceIndex, resp.Name);
            }

            return resp;
        }

        //commit docs
        //TODO: untyped commit/delete
        public bool Commit<T>(T document)
            where T : class, IDocument
        {
            return CommitAsync(document).Result;
        }
        public async Task<bool> CommitAsync<T>(T document)
            where T : class, IDocument
        {
            if (!TryGetIndexForDocument(document, out var idx))
            {
                if (!AllowAutomaticIndexCreation)
                {
                    Log.Warning("Could not identify index for provided document {0} and cluster settings do not allow for automatic index creation.", document.Id);
                    return false;
                }
                else
                {
                    Log.Warning("No index found for document with type {0}, but cluster settings allow for automatic index creation.", document.GetType().FullName);
                    var indexConfig = new IndexConfig(document.GetType().FullName, document.GetType().FullName, indexAsType: document.IndexAsType);
                    idx = await CreateIndexAsync(indexConfig);
                }
            }
            var idxTarget = IndicesByType[idx.IndexAsType].FirstOrDefault()?.Name ?? idx.Name;
            Log.Verbose("Attempting to index document {0} to index {1}", document.Id, idxTarget);
            var res = await _client.IndexAsync(
                document,
                x => x
                    .Index(Nest.Indices.Index(idxTarget))
                    .Refresh(idx.ForceRefreshOnDocumentCommit
                        ? Refresh.True
                        : Refresh.False));
            Log.Verbose("Index operation complete for document {0} to index {1}", document.Id, idxTarget);

            if (res.IsValid is not true)
            {
                const string msg = @"Could not index document {0} to index {1}, and could not recover from error.  Review logs and try again.";
                Log.Error(msg, document.Id, idx.Name);
                Log.Error(res.ServerError?.Error?.Reason);
                Log.Error(res.OriginalException.Message);
                Log.Error(res.OriginalException.StackTrace);

                throw new InvalidOperationException(
                    string.Format(msg, document.Id, idx.Name));
            }

            return res.IsValid;
        }
        public bool Commit<T>(IEnumerable<T> documents)
            where T : class, IDocument
        {
            return CommitAsync(documents).Result;
        }
        public async Task<bool> CommitAsync<T>(IEnumerable<T> documents)
            where T : class, IDocument
        {

            var bulk = new BulkDescriptor();
            Dictionary<string, string> idx_as_type_cache = new Dictionary<string, string>();
            foreach(var document in documents)
            {

                if (!TryGetIndexForDocument(document, out var idx))
                {
                    Log.Warning("Could not identify index for provided document {0}", document.Id);
                    return false;
                }
                var idxTarget = idx.Name;
                if (!string.IsNullOrWhiteSpace(idx.IndexAsType))
                {
                    if (!idx_as_type_cache.TryGetValue(idx.Name, out idxTarget))
                    {
                        idxTarget = IndicesByType[idx.IndexAsType].FirstOrDefault()?.Name ?? idx.Name;
                        idx_as_type_cache[idx.Name] = idxTarget;
                    }
                }
                bulk.Index<object>(x => x
                    .Index(idxTarget)
                    .Id(document.Id)
                    .Document(document))
                    .Refresh(idx.ForceRefreshOnDocumentCommit
                        ? Refresh.True
                        : Refresh.False);
            }

            Log.Verbose("Indexing {0} documents", documents.Count());

            var resp = await _client.BulkAsync(bulk);

            //There are issues with cross-version compatability and error detection on bulk methods - many successful index ops report unknown errors.
            //Taking a simpler, more naive path towards success detection until a new release of the client that functions correctly.

            //Log.Verbose("Bulk index attempt complete with {0} errors.", resp.ItemsWithErrors.Count());

            //if (resp.Errors is true)
            //{
            //    if (resp.ItemsWithErrors?.Any() is true)
            //    {
            //        Log.Error("Some items - {0} of {1} - failed to complete, with the following errors:", resp.ItemsWithErrors.Count(), resp.Items.Count());
            //        foreach(var err in resp.ItemsWithErrors)
            //        {
            //            Log.Error("Item Id: {0}, Index: {1}", err?.Id, err?.Index);
            //            Log.Error(err?.Error?.Reason);
            //            Log.Error(err?.Error?.CausedBy?.Reason);
            //            Log.Error(err?.Error?.StackTrace);
            //        }

            //        return false;
            //    }
            //    if (resp.IsValid is not true)
            //    {
            //        Log.Error("Error in index attempt:");
            //        Log.Error(resp.ServerError?.Error?.Reason);
            //        Log.Error(resp.OriginalException?.Message);
            //        Log.Error(resp.OriginalException?.StackTrace);
            //    }
            //}

            return resp?.Items?.Count.Equals(documents?.Count()) ?? false;
        }

        public bool Commit(BaseDocument document)
        {
            return CommitAsync(document).Result;
        }
        public async Task<bool> CommitAsync(object document)
        {
            if (!document.GetType().IsAssignableTo(typeof(BaseDocument)))
            {
                Log.Warning("Type {0} could not be indexed - only types that inherit from {1} are allowed.", document.GetType().FullName, typeof(seaq.BaseDocument).FullName);
                return false;
            }
            var doc = document as BaseDocument;
            if (doc == null)
            {
                Log.Warning("Provided document of type {0} could not be cast as {1}.", document.GetType().FullName, typeof(seaq.BaseDocument).FullName);
                return false;
            }
            if (!TryGetIndexForDocument(doc, out var idx))
            {
                if (!AllowAutomaticIndexCreation)
                {
                    Log.Warning("Could not identify index for provided document {0} and cluster settings do not allow for automatic index creation.", doc.Id);
                    return false;
                }
                else
                {
                    Log.Warning("No index found for document with type {0}, but cluster settings allow for automatic index creation.", doc.GetType().FullName);
                    var indexConfig = new IndexConfig(doc.GetType().FullName, doc.GetType().FullName);
                    idx = await CreateIndexAsync(indexConfig);
                }
            }

            Log.Verbose("Attempting to index document {0} to index {1}", doc.Id, idx.Name);
            var res = await _client.IndexAsync(
                doc,
                x => x
                    .Index(Nest.Indices.Index(idx.Name))
                    .Refresh(idx.ForceRefreshOnDocumentCommit
                        ? Refresh.True
                        : Refresh.False));
            Log.Verbose("Index operation complete for document {0} to index {1}", doc.Id, idx.Name);

            if (res.IsValid is not true)
            {
                const string msg = @"Could not index document {0} to index {1}, and could not recover from error.  Review logs and try again.";
                Log.Error(msg, doc.Id, idx.Name);
                Log.Error(res.ServerError?.Error?.Reason);
                Log.Error(res.OriginalException.Message);
                Log.Error(res.OriginalException.StackTrace);

                throw new InvalidOperationException(
                    string.Format(msg, doc.Id, idx.Name));
            }

            return res.IsValid;
        }
        public bool Commit(IEnumerable<object> documents)
        {
            return CommitAsync(documents).Result;
        }
        public async Task<bool> CommitAsync(IEnumerable<object> documents)
        {
            var bulk = new BulkDescriptor();
            Dictionary<string, string> idx_as_type_cache = new Dictionary<string, string>();
            foreach (var document in documents)
            {
                if (!document.GetType().IsAssignableTo(typeof(BaseDocument)))
                {
                    Log.Warning("Type {0} could not be indexed - only types that inherit from {1} are allowed.", document.GetType().FullName, typeof(seaq.BaseDocument).FullName);
                    return false;
                }
                var doc = document as BaseDocument;
                if (doc == null)
                {
                    Log.Warning("Provided document of type {0} could not be cast as {1}.", document.GetType().FullName, typeof(seaq.BaseDocument).FullName);
                    return false;
                }
                if (!TryGetIndexForDocument(doc, out var idx))
                {
                    Log.Warning("Could not identify index for provided document {0}", doc.Id);
                    return false;
                }

                var idxTarget = idx.Name;
                if (!string.IsNullOrWhiteSpace(idx.IndexAsType))
                {
                    if (!idx_as_type_cache.TryGetValue(idx.Name, out idxTarget))
                    {
                        idxTarget = IndicesByType[idx.IndexAsType].FirstOrDefault()?.Name ?? idx.Name;
                        idx_as_type_cache[idx.Name] = idxTarget;
                    }
                }
                bulk.Index<object>(x => x
                    .Index(idxTarget)
                    .Id(doc.Id)
                    .Document(document))
                    .Refresh(idx.ForceRefreshOnDocumentCommit
                        ? Refresh.True
                        : Refresh.False);
            }

            Log.Verbose("Indexing {0} documents", documents.Count());

            var resp = await _client.BulkAsync(bulk);

            //There are issues with cross-version compatability and error detection on bulk methods - many successful index ops report unknown errors.
            //Taking a simpler, more naive path towards success detection until a new release of the client that functions correctly.

            //Log.Verbose("Bulk index attempt complete with {0} errors.", resp.ItemsWithErrors.Count());

            //if (resp.Errors is true)
            //{
            //    if (resp.ItemsWithErrors?.Any() is true)
            //    {
            //        Log.Error("Some items - {0} of {1} - failed to complete, with the following errors:", resp.ItemsWithErrors.Count(), resp.Items.Count());
            //        foreach(var err in resp.ItemsWithErrors)
            //        {
            //            Log.Error("Item Id: {0}, Index: {1}", err?.Id, err?.Index);
            //            Log.Error(err?.Error?.Reason);
            //            Log.Error(err?.Error?.CausedBy?.Reason);
            //            Log.Error(err?.Error?.StackTrace);
            //        }

            //        return false;
            //    }
            //    if (resp.IsValid is not true)
            //    {
            //        Log.Error("Error in index attempt:");
            //        Log.Error(resp.ServerError?.Error?.Reason);
            //        Log.Error(resp.OriginalException?.Message);
            //        Log.Error(resp.OriginalException?.StackTrace);
            //    }
            //}

            return resp?.Items?.Count.Equals(documents?.Count()) ?? false;
        }


        //get index schema
        public Index GetIndexDefinition(string indexName)
        {
            return GetIndexDefinitionAsync(indexName).Result;
        }
        public async Task<Index> GetIndexDefinitionAsync(string indexName)
        {
            var query = new GetIndexRequest(Nest.Indices.Index(indexName));
            var resp = await _client.Indices
                .GetAsync(query);

            if (resp.IsValid is not true)
            {
                Log.Error("Could not retrieve index definition.");
                Log.Error(resp.ServerError?.Error?.Reason);
                Log.Error(resp.OriginalException.Message);
                Log.Error(resp.OriginalException.StackTrace);
            }

            return resp.Indices.Select(Index.Create).FirstOrDefault();
        }

        //update index schema
        public Index UpdateIndexDefinition(Index index)
        {
            return UpdateIndexDefinitionAsync(index)?.Result;
        }
        public async Task<Index> UpdateIndexDefinitionAsync(Index index)
        {
            if (!_indices.ContainsKey(index.Name))
            {
                Log.Warning("Attempt to update index {0} failed - could not find it on cluster {1}", index.Name, ClusterScope);
                await Task.CompletedTask;
                return null;
            }

            var getMapping = new GetMappingRequest(Nest.Indices.Index(index.Name));
            var mapping = await _client.Indices.GetMappingAsync(getMapping);

            if (mapping.IsValid is not true)
            {
                var msg = @"Could not fetch current mapping for index {0}";
                Log.Error(msg, index.Name);
                Log.Error(mapping.ServerError?.Error?.Reason);
                Log.Error(mapping.OriginalException?.Message);
                Log.Error(mapping.OriginalException?.StackTrace);
                throw new InvalidOperationException(string.Format(msg, index.Name));
            }

            var meta = (mapping?.Indices?.FirstOrDefault())?.Value?.Mappings?.Meta;
            if (meta is null)
            {
                meta = new Dictionary<string, object>();
            }

            meta[Constants.Indices.Meta.SchemaKey] = index;
            var putMapping = new PutMappingRequest(Nest.Indices.Index(index.Name));
            putMapping.Meta = meta;

            var resp = await _client.Indices.PutMappingAsync(putMapping);

            if (resp.IsValid is not true)
            {
                var msg = @"Could not update mapping for index {0}";
                Log.Error(msg, index.Name);
                Log.Error(mapping.ServerError?.Error?.Reason);
                Log.Error(mapping.OriginalException?.Message);
                Log.Error(mapping.OriginalException?.StackTrace);
                throw new InvalidOperationException(string.Format(msg, index.Name));
            }

            var server_index = _client.Indices.Get(index.Name)?.Indices?.FirstOrDefault();

            if (!server_index.HasValue)
            {
                const string msg = @"Index {0} mapping update reported success, but could not retrieve index definition from server.  This cannot be automatically resolved - suggest handling with index cache refresh.";
                Log.Error(msg, index.Name);
                throw new InvalidOperationException(string.Format(msg, index.Name));
            }

            index = Index.Create(server_index.Value);

            _indices[index.Name] = index;

            return index;
        }
        
        public Index UpdateIndexField(string indexName, Field field)
        {
            return UpdateIndexFieldAsync(indexName, field).Result;
        }
        public async Task<Index> UpdateIndexFieldAsync(string indexName, Field field)
        {

            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to UpdateIndexField for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }

            var f = index.Fields.ToList();
            var idx = f.FindIndex(x => x.Name.Equals(field.Name, StringComparison.OrdinalIgnoreCase));
            if (idx == -1)
            {
                Log.Warning("Could not find field {0} on index {1} - UpdateIndexField failed.", field.Name, indexName);
                await Task.CompletedTask;
                return null;
            }

            field.Merge(f[idx]);
            index.Fields = f;

            return await UpdateIndexDefinitionAsync(index);
        }
        
        public Index DeprecateIndex(string indexName, string deprecationMessage)
        {
            return DeprecateIndexAsync(indexName, deprecationMessage).Result;
        }
        public async Task<Index> DeprecateIndexAsync(string indexName, string deprecationMessage)
        {
            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to DeprecateIndex for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }

            index.IsDeprecated = true;
            index.DeprecationMessage = deprecationMessage;

            return await UpdateIndexDefinitionAsync(index);
        }
        public Index UnDeprecateIndex(string indexName)
        {
            return UnDeprecateIndexAsync(indexName).Result;
        }
        public async Task<Index> UnDeprecateIndexAsync(string indexName)
        {
            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to UnDeprecateIndex for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }

            index.IsDeprecated = false;
            index.DeprecationMessage = null;

            return await UpdateIndexDefinitionAsync(index);
        }

        public Index HideIndex(string indexName)
        {
            return HideIndexAsync(indexName).Result;
        }
        public async Task<Index> HideIndexAsync(string indexName)
        {
            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to HideIndex for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }

            index.IsHidden = true;

            return await UpdateIndexDefinitionAsync(index);
        }
        public Index UnHideIndex(string indexName)
        {
            return UnHideIndexAsync(indexName).Result;
        }
        public async Task<Index> UnHideIndexAsync(string indexName)
        {
            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to UnHideIndex for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }

            index.IsHidden = false;

            return await UpdateIndexDefinitionAsync(index);
        }

        public Index ForceRefreshOnDocumentCommit(
            string indexName)
        {
            return ForceRefreshOnDocumentCommitAsync(indexName).Result;
        }
        public async Task<Index> ForceRefreshOnDocumentCommitAsync(
            string indexName)
        {
            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to ForceRefreshOnDocumentCommitAsync for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }

            index.ForceRefreshOnDocumentCommit = true;

            return await UpdateIndexDefinitionAsync(index);
        }
        public Index DoNotForceRefreshOnDocumentCommit(
            string indexName)
        {
            return DoNotForceRefreshOnDocumentCommitAsync(indexName).Result;
        }
        public async Task<Index> DoNotForceRefreshOnDocumentCommitAsync(
            string indexName)
        {
            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to DoNotForceRefreshOnDocumentCommitAsync for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }

            index.ForceRefreshOnDocumentCommit = false;

            return await UpdateIndexDefinitionAsync(index);
        }
        public Index IncludeIndexInGlobalSearch(string indexName)
        {
            return IncludeIndexInGlobalSearchAsync(indexName).Result;
        }
        public async Task<Index> IncludeIndexInGlobalSearchAsync(string indexName)
        {
            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to IncludeIndexInGlobalSearch for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }

            index.ReturnInGlobalSearch = true;

            return await UpdateIndexDefinitionAsync(index);
        }
        public Index ExcludeIndexFromGlobalSearch(string indexName)
        {
            return ExcludeIndexFromGlobalSearchAsync(indexName).Result;
        }
        public async Task<Index> ExcludeIndexFromGlobalSearchAsync(string indexName)
        {
            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to ExcludeIndexFromGlobalSearch for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }

            index.ReturnInGlobalSearch = false;

            return await UpdateIndexDefinitionAsync(index);
        }
        public Index SetIndexObjectLabel(string indexName, string label)
        {
            return SetIndexObjectLabelAsync(indexName, label).Result;
        }
        public async Task<Index> SetIndexObjectLabelAsync(string indexName, string label)
        {
            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to SetIndexObjectLabel for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }

            index.ObjectLabel = label;

            return await UpdateIndexDefinitionAsync(index);
        }
        public Index SetIndexObjectLabelPlural(string indexName, string label)
        {
            return SetIndexObjectLabelPluralAsync(indexName, label).Result;
        }
        public async Task<Index> SetIndexObjectLabelPluralAsync(string indexName, string label)
        {
            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to SetIndexObjectLabelPlural for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }

            index.ObjectLabelPlural = label;

            return await UpdateIndexDefinitionAsync(index);
        }
        public Index SetIndexPrimaryField(string indexName, string fieldName)
        {
            return SetIndexPrimaryFieldAsync(indexName, fieldName).Result;
        }
        public async Task<Index> SetIndexPrimaryFieldAsync(string indexName, string fieldName)
        {
            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to SetIndexPrimaryField for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }

            index.PrimaryField = fieldName;

            return await UpdateIndexDefinitionAsync(index);
        }
        public Index SetIndexPrimaryFieldLabel(string indexName, string label)
        {
            return SetIndexPrimaryFieldLabelAsync(indexName, label).Result;
        }
        public async Task<Index> SetIndexPrimaryFieldLabelAsync(string indexName, string label)
        {
            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to SetIndexPrimaryFieldLabel for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }

            index.PrimaryFieldLabel = label;

            return await UpdateIndexDefinitionAsync(index);
        }
        public Index SetIndexSecondaryField(string indexName, string fieldName)
        {
            return SetIndexSecondaryFieldAsync(indexName, fieldName).Result;
        }
        public async Task<Index> SetIndexSecondaryFieldAsync(string indexName, string fieldName)
        {
            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to SetIndexSecondaryField for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }

            index.SecondaryField = fieldName;

            return await UpdateIndexDefinitionAsync(index);
        }
        public Index SetIndexSecondaryFieldLabel(string indexName, string label)
        {
            return SetIndexSecondaryFieldLabelAsync(indexName, label).Result;
        }
        public async Task<Index> SetIndexSecondaryFieldLabelAsync(string indexName, string label)
        {
            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to SetIndexSecondaryFieldLabel for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }

            index.SecondaryFieldLabel = label;

            return await UpdateIndexDefinitionAsync(index);
        }
        public Index SetIndexMeta(string indexName, Dictionary<string, object> meta)
        {
            return SetIndexMetaAsync(indexName, meta).Result;
        }
        public async Task<Index> SetIndexMetaAsync(string indexName, Dictionary<string, object> meta)
        {
            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to SetIndexMeta for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }
            if (index?.Meta?.Keys?.Count > 0)
            {
                Log.Warning("Could not set index meta - a value already exists.  Use either AppendIndexMeta or the ReplaceIndexMeta to proceed.");
                await Task.CompletedTask;
                return index;
            }

            index.Meta = meta;

            return await UpdateIndexDefinitionAsync(index);
        }
        public Index ReplaceIndexMeta(string indexName, Dictionary<string, object> meta)
        {
            return ReplaceIndexMetaAsync(indexName, meta).Result;
        }
        public async Task<Index> ReplaceIndexMetaAsync(string indexName, Dictionary<string, object> meta)
        {
            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to ReplaceIndexMeta for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }

            index.Meta = meta;

            return await UpdateIndexDefinitionAsync(index);
        }
        public Index AppendIndexMeta(string indexName, Dictionary<string, object> meta)
        {
            return AppendIndexMetaAsync(indexName, meta).Result;
        }
        public async Task<Index> AppendIndexMetaAsync(string indexName, Dictionary<string, object> meta)
        {
            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to AppendIndexMeta for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }
            foreach(var key in meta.Keys)
            {
                index.Meta[key] = meta[key];
            }

            return await UpdateIndexDefinitionAsync(index);
        }
        public Index DeleteIndexMeta(string indexName)
        {
            return DeleteIndexMetaAsync(indexName).Result;
        }
        public async Task<Index> DeleteIndexMetaAsync(string indexName)
        {
            if (!_indices.TryGetValue(indexName, out var index))
            {
                Log.Warning("Attempt to DeleteIndexMeta for index {0} failed - could not find it on cluster {1}", indexName, ClusterScope);
                await Task.CompletedTask;
                return null;
            }

            index.Meta = new Dictionary<string, object>();

            return await UpdateIndexDefinitionAsync(index);
        }

        //delete docs
        public bool Delete<T>(T document)
            where T : BaseDocument
        {
            return DeleteAsync(document).Result;
        }
        public async Task<bool> DeleteAsync<T>(T document)
            where T : BaseDocument
        {
            if (!TryGetIndexForDocument(document, out var idx))
            {
                Log.Warning("Could not identify index for provided document {0}", document.Id);
                return false;
            }

            Log.Verbose("Attempting to delete document {0} from index {1}", document.Id, idx.Name);

            var idxTarget = IndicesByType[idx.IndexAsType].FirstOrDefault()?.Name ?? idx.Name;
            var resp = await _client.DeleteAsync<T>(
                document.Id, 
                x => x
                    .Index(Nest.Indices.Index(idxTarget))
                    .Refresh(idx.ForceRefreshOnDocumentCommit
                        ? Refresh.True
                        : Refresh.False));

            Log.Verbose("Delete operation complete for document {0} on index {1}", document.Id, idx.Name);


            if (resp.IsValid is not true)
            {
                if (resp.ApiCall.HttpStatusCode == 404)
                {
                    const string msg = @"Could not find document {0} on index {1}";
                    Log.Warning(msg, document.Id, idxTarget);
                    return false;
                }
                else
                {
                    const string msg = @"Could not delete document {0} from index {1}, and could not recover from error.  Review logs and try again.";
                    Log.Error(msg, document.Id, idxTarget);
                    Log.Error(resp?.ServerError?.Error?.Reason);
                    Log.Error(resp?.OriginalException.Message);
                    Log.Error(resp?.OriginalException.StackTrace);

                    throw new InvalidOperationException(
                        string.Format(msg, document.Id, idxTarget));
                }
            }

            return resp.IsValid;
        }
        public bool Delete<T>(IEnumerable<T> documents)
            where T : BaseDocument
        {
            return DeleteAsync(documents).Result;
        }
        public async Task<bool> DeleteAsync<T>(IEnumerable<T> documents)
            where T : BaseDocument
        {
            var bulk = new BulkDescriptor();
            Dictionary<string, string> idx_as_type_cache = new Dictionary<string, string>();
            foreach (var document in documents)
            {

                if (!TryGetIndexForDocument(document, out var idx))
                {
                    Log.Warning("Could not identify index for provided document {0}", document.Id);
                    return false;
                }
                var idxTarget = idx.Name;
                if (!string.IsNullOrWhiteSpace(idx.IndexAsType))
                {
                    if (!idx_as_type_cache.TryGetValue(idx.Name, out idxTarget))
                    {
                        idxTarget = IndicesByType[idx.IndexAsType].FirstOrDefault()?.Name ?? idx.Name;
                        idx_as_type_cache[idx.Name] = idxTarget;
                    }
                }

                bulk.Delete<BaseDocument>(x => x
                    .Index(idxTarget)
                    .Id(document.Id)
                    .Document(document))
                    .Refresh(idx.ForceRefreshOnDocumentCommit
                        ? Refresh.True
                        : Refresh.False);
            }

            Log.Verbose("Deleting {0} documents", documents.Count());

            var resp = await _client.BulkAsync(bulk);

            //There are issues with cross-version compatability and error detection on bulk methods - many successful index ops report unknown errors.
            //Taking a simpler, more naive path towards success detection until a new release of the client that functions correctly.

            //Log.Verbose("Bulk delete attempt complete with {0} errors.", resp.ItemsWithErrors.Count());

            //List<BulkResponseItemBase> errors = resp.ItemsWithErrors.ToList();
            //if (resp.Items.Any(x => x?.Status == 404))
            //{
            //    var notFound = resp.Items.Where(x => x?.Status == 404);

            //    foreach(var nf in notFound)
            //    {
            //        const string msg = @"Could not find document {0} on index {1}";
            //        Log.Warning(msg, nf.Id, nf.Index);
            //        errors.Add(nf);
            //    }
            //}

            //if (resp.ItemsWithErrors?.Any() is true)
            //{
            //    Log.Error("Some items - {0} of {1} - failed to complete, with the following errors:", resp.ItemsWithErrors.Count(), resp.ItemsWithErrors.Count());
            //    foreach (var err in resp.ItemsWithErrors)
            //    {
            //        Log.Error("Item Id: {0}, Index: {1}", err?.Id, err?.Index);
            //        Log.Error(err?.Error?.Reason);
            //        Log.Error(err?.Error?.CausedBy?.Reason);
            //        Log.Error(err?.Error?.StackTrace);
            //    }

            //    return false;
            //}
            //if (resp.IsValid is not true)
            //{
            //    Log.Error("Error in delete attempt:");
            //    Log.Error(resp.ServerError?.Error?.Reason);
            //    Log.Error(resp.OriginalException?.Message);
            //    Log.Error(resp.OriginalException?.StackTrace);
            //}

            return resp?.Items?.Count.Equals(documents?.Count()) ?? false;
        }

        public bool Delete(BaseDocument document)
        {
            return DeleteAsync(document).Result;
        }
        public async Task<bool> DeleteAsync(object document)
        {
            if (!document.GetType().IsAssignableTo(typeof(BaseDocument)))
            {
                Log.Warning("Type {0} could not be indexed - only types that inherit from {1} are allowed.", document.GetType().FullName, typeof(seaq.BaseDocument).FullName);
                return false;
            }
            var doc = document as BaseDocument;
            if (doc == null)
            {
                Log.Warning("Provided document of type {0} could not be cast as {1}.", document.GetType().FullName, typeof(seaq.BaseDocument).FullName);
                return false;
            }
            if (!TryGetIndexForDocument(doc, out var idx))
            {
                Log.Warning("Could not identify index for provided document {0}", doc.Id);
                return false;
            }

            Log.Verbose("Attempting to delete document {0} from index {1}", doc.Id, idx.Name);

            var idxTarget = IndicesByType[idx.IndexAsType].FirstOrDefault()?.Name ?? idx.Name;
            var resp = await _client.DeleteAsync<BaseDocument>(
                doc.Id,
                x => x
                    .Index(Nest.Indices.Index(idxTarget))
                    .Refresh(idx.ForceRefreshOnDocumentCommit
                        ? Refresh.True
                        : Refresh.False));

            Log.Verbose("Delete operation complete for document {0} on index {1}", doc.Id, idxTarget);


            if (resp.IsValid is not true)
            {
                if (resp.ApiCall.HttpStatusCode == 404)
                {
                    const string msg = @"Could not find document {0} on index {1}";
                    Log.Warning(msg, doc.Id, idxTarget);
                    return false;
                }
                else
                {
                    const string msg = @"Could not delete document {0} from index {1}, and could not recover from error.  Review logs and try again.";
                    Log.Error(msg, doc.Id, idxTarget);
                    Log.Error(resp?.ServerError?.Error?.Reason);
                    Log.Error(resp?.OriginalException.Message);
                    Log.Error(resp?.OriginalException.StackTrace);

                    throw new InvalidOperationException(
                        string.Format(msg, doc.Id, idxTarget));
                }
            }

            return resp.IsValid;
        }
        public bool Delete(IEnumerable<BaseDocument> documents)
        {
            return DeleteAsync(documents).Result;
        }
        public async Task<bool> DeleteAsync(IEnumerable<object> documents)
        {
            var bulk = new BulkDescriptor();
            Dictionary<string, string> idx_as_type_cache = new Dictionary<string, string>();
            foreach (var document in documents)
            {
                if (!document.GetType().IsAssignableTo(typeof(BaseDocument)))
                {
                    Log.Warning("Type {0} could not be indexed - only types that inherit from {1} are allowed.", document.GetType().FullName, typeof(seaq.BaseDocument).FullName);
                    return false;
                }
                var doc = document as BaseDocument;
                if (doc == null)
                {
                    Log.Warning("Provided document of type {0} could not be cast as {1}.", document.GetType().FullName, typeof(seaq.BaseDocument).FullName);
                    return false;
                }
                if (!TryGetIndexForDocument(doc, out var idx))
                {
                    Log.Warning("Could not identify index for provided document {0}", doc.Id);
                    return false;
                }

                var idxTarget = idx.Name;
                if (!string.IsNullOrWhiteSpace(idx.IndexAsType))
                {
                    if (!idx_as_type_cache.TryGetValue(idx.Name, out idxTarget))
                    {
                        idxTarget = IndicesByType[idx.IndexAsType].FirstOrDefault()?.Name ?? idx.Name;
                        idx_as_type_cache[idx.Name] = idxTarget;
                    }
                }
                bulk.Delete<BaseDocument>(x => x
                    .Index(idxTarget)
                    .Id(doc.Id)
                    .Document(doc))
                    .Refresh(idx.ForceRefreshOnDocumentCommit
                        ? Refresh.True
                        : Refresh.False);
            }

            Log.Verbose("Deleting {0} documents", documents.Count());

            var resp = await _client.BulkAsync(bulk);

            //There are issues with cross-version compatability and error detection on bulk methods - many successful index ops report unknown errors.
            //Taking a simpler, more naive path towards success detection until a new release of the client that functions correctly.

            //Log.Verbose("Bulk delete attempt complete with {0} errors.", resp.ItemsWithErrors.Count());

            //List<BulkResponseItemBase> errors = resp.ItemsWithErrors.ToList();
            //if (resp.Items.Any(x => x?.Status == 404))
            //{
            //    var notFound = resp.Items.Where(x => x?.Status == 404);

            //    foreach(var nf in notFound)
            //    {
            //        const string msg = @"Could not find document {0} on index {1}";
            //        Log.Warning(msg, nf.Id, nf.Index);
            //        errors.Add(nf);
            //    }
            //}

            //if (resp.ItemsWithErrors?.Any() is true)
            //{
            //    Log.Error("Some items - {0} of {1} - failed to complete, with the following errors:", resp.ItemsWithErrors.Count(), resp.ItemsWithErrors.Count());
            //    foreach (var err in resp.ItemsWithErrors)
            //    {
            //        Log.Error("Item Id: {0}, Index: {1}", err?.Id, err?.Index);
            //        Log.Error(err?.Error?.Reason);
            //        Log.Error(err?.Error?.CausedBy?.Reason);
            //        Log.Error(err?.Error?.StackTrace);
            //    }

            //    return false;
            //}
            //if (resp.IsValid is not true)
            //{
            //    Log.Error("Error in delete attempt:");
            //    Log.Error(resp.ServerError?.Error?.Reason);
            //    Log.Error(resp.OriginalException?.Message);
            //    Log.Error(resp.OriginalException?.StackTrace);
            //}

            return resp?.Items?.Count.Equals(documents?.Count()) ?? false;
        }

        //query
        public ISeaqQueryResults<T> Query<T>(ISeaqQuery<T> query)
            where T : BaseDocument
        {
            if (query.Criteria is null)
            {
                const string msg = @"Can not process query - criteria object is null.";
                Log.Error(msg);
                throw new ArgumentNullException(string.Format(msg));
            }
            Log.Verbose("Executing sync query against cluster {0}, indices {1}", ClusterScope, string.Join(", ", query.Criteria.Indices ?? Array.Empty<string>()));

            query.Criteria.ApplyClusterSettings(this);

            return query.Execute(_client);
        }
        public TResp Query<TResp>(ISeaqQuery query)
            where TResp : class, ISeaqQueryResults
        {
            if (query.Criteria is null)
            {
                const string msg = @"Can not process query - criteria object is null.";
                Log.Error(msg);
                throw new ArgumentNullException(string.Format(msg));
            }
            Log.Verbose("Executing sync query against cluster {0}, indices {1}", ClusterScope, string.Join(", ", query.Criteria.Indices));

            query.Criteria.ApplyClusterSettings(this);

            return query.Execute(_client) as TResp;
        }

        public async Task<ISeaqQueryResults<T>> QueryAsync<T>(ISeaqQuery<T> query)
            where T : BaseDocument
        {
            if (query.Criteria is null)
            {
                const string msg = @"Can not process query - criteria object is null.";
                Log.Error(msg);
                throw new ArgumentNullException(string.Format(msg));
            }
            Log.Verbose("Executing async query against cluster {0}, indices {1}", ClusterScope, string.Join(", ", query.Criteria.Indices));

            query.Criteria.ApplyClusterSettings(this);

            return await query.ExecuteAsync(_client);
        }
        public async Task<TResp> QueryAsync<TResp>(ISeaqQuery query)
            where TResp : class, ISeaqQueryResults
        {
            if (query.Criteria is null)
            {
                const string msg = @"Can not process query - criteria object is null.";
                Log.Error(msg);
                throw new ArgumentNullException(string.Format(msg));
            }
            
            Log.Verbose("Executing sync query against cluster {0}, indices {1}", ClusterScope, string.Join(", ", query.Criteria.Indices));

            query.Criteria.ApplyClusterSettings(this);
            
            return await query.ExecuteAsync(_client) as TResp;
        }


        private bool TryGetIndexForDocument<T>(T document, out Index index)
            where T : IDocument
        {

            var resp = _indices.TryGetValue(document.IndexName ?? "", out index);


            if (document?.IndexName?.StartsWith(ClusterScope) is not true)
            {
                var index_name_adj = string.Join(Constants.Indices.NamePartSeparator, ClusterScope, document.IndexName);
                Log.Debug("Provided index name {0} doesn't begin with ClusterScope {1} as expected.  Coerced name to {2} to match expectations", document.IndexName, ClusterScope, index_name_adj);
                resp = _indices.TryGetValue(index_name_adj ?? "", out index);
            }

            if (resp is not true)
            {
                var byType = IndicesByType[document.Type];

                if (byType?.Any() is not true)
                {
                    const string msg = @"Document's specified index {0} does not exist in the cluster.";
                    Log.Warning(msg, document.IndexName);
                    return false;
                }

                if (byType?.Count() > 1)
                {
                    const string msg = @"Document's sepcified type {0} maps to multiple indices, and no index is specified.  Cannot proceed.";
                    Log.Warning(msg, document.Type);
                    return false;
                }

                index = byType.First();
                return true;
            }
            return resp;
        }

        private void ConfigureEventHandlers()
        {
            Log.Verbose("Configuring event handlers...");
            EventHandler CacheInitializing = (sender, args) =>
            {
                IsCacheInitializing = true;
                Log.Debug("{0} index cache init begun at {1}", ClusterScope, DateTime.UtcNow);
            };
            EventHandler CacheInitialized = (sender, args) =>
            {
                CacheInitializedUtc = DateTime.UtcNow;
                IsCacheInitializing = false;
                Log.Debug("{0} index cache initialized at {1}", ClusterScope, CacheInitializedUtc);
            };
            EventHandler CacheRefreshing = (sender, args) =>
            {
                IsCacheRefreshing = true;
                Log.Debug("{0} index cache refresh begun at {1}", ClusterScope, DateTime.UtcNow);
            };
            EventHandler CacheRefreshed = (sender, args) =>
            {
                CacheRefreshedUtc = DateTime.UtcNow;
                IsCacheRefreshing = false;
                Log.Debug("{0} index cache refreshed at {1}", ClusterScope, CacheRefreshedUtc);
            };

            IndexCacheInitializing += CacheInitializing;
            IndexCacheInitialized += CacheInitialized;
            IndexCacheRefreshing += CacheRefreshing;
            IndexCacheRefreshed += CacheRefreshed;
            Log.Verbose("Event handlers configured.");
        }

        private Type TryGetSearchType(
            string typeFullName)
        {
            if (_searchableTypes.TryGetValue(typeFullName, out var type))
            {
                return type;
            }
            else
            {
                return typeof(BaseDocument);
            }
        }
        private static ElasticClient BuildClient(
            ClusterArgs args,
            ISeaqElasticsearchSerializer serializer)
        {
            var pool = new SingleNodeConnectionPool(
                new Uri(args.Url));

            var settings = new ConnectionSettings(
                pool,
                (a, b) => serializer);

            if (args.EnableVersionCompatabilityHeader is true)
                settings.EnableApiVersioningHeader();

            if (!string.IsNullOrWhiteSpace(args.Username) && !string.IsNullOrWhiteSpace(args.Password))
            {
                settings.BasicAuthentication(args.Username, args.Password);
                ///this is the great big hammer to break out when things aren't working - 
                ///it bypasses certificate validation entirely, which is necessary for local self-signed certs,
                ///but can be a GIANT security risk otherwise.  Only used for debugging for a reason.
#if DEBUG
                Log.Debug("DEBUG mode enabled.  Ignoring server certificate validation and enabling additional Elasticsearch debug messages.");
                settings.ServerCertificateValidationCallback((a, b, c, d) => true);
                settings.EnableDebugMode();
#endif
            }
            if (args.BypassCertificateValidation)
            {
                Log.Warning("Bypassing SSL Certificate Validation.  This is necessary for self-signed and other untrusted certificates, but can pose a large security risk.  Make sure that this is intentional.");
                settings.ServerCertificateValidationCallback((a, b, c, d) => true);
            }

            return new ElasticClient(settings);
        }
    
    
        private async Task HydrateInternalStore()
        {
            await CommitAsync(_indices.Values);
        }
        private async Task DeleteInternalStore()
        {
            await DeleteIndexAsync(typeof(seaq.Index).FullName);
        }
        private async Task SaveToInternalStore(Index idx)
        {
            await CommitAsync(idx);
        }
        private async Task DeleteFromInternalStore(Index idx)
        {
            await DeleteAsync(idx);
        }
        private async Task RefreshFromInternalStore()
        {
            var resp = await _client.SearchAsync<Index>(x => x.Index(InternalStoreIndex).MatchAll());
            if (resp.IsValid)
            {
                if (_indices?.Any() is not true)
                    _indices = new Dictionary<string, Index>();
                resp.Documents.ToList().ForEach(x =>
                {
                    if (!_indices.ContainsKey(x.Name))
                        _indices[x.Name] = x;
                });
            }
        }

    }
}
