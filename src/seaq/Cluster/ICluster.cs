using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace seaq
{
    public interface ICluster
    {
        IAggregationCache AggregationCache { get; }
        bool AllowAutomaticIndexCreation { get; }
        DateTime? CacheInitializedUtc { get; }
        DateTime? CacheRefreshedUtc { get; }
        string ClusterScope { get; set; }
        IEnumerable<Index> DeprecatedIndices { get; }
        IEnumerable<Index> GlobalResultIndices { get; }
        IEnumerable<Index> HiddenIndices { get; }
        IEnumerable<Index> Indices { get; }
        ILookup<string, Index> IndicesByType { get; }
        string InternalStoreIndex { get; }
        bool IsCacheInitializing { get; }
        bool IsCacheRefreshing { get; }
        IEnumerable<Index> NotDeprecatedIndices { get; }
        IEnumerable<Index> NotGlobalResultIndices { get; }
        IEnumerable<Index> NotHiddenIndices { get; }
        IEnumerable<Type> SearchableTypes { get; }

        event EventHandler IndexCacheInitialized;
        event EventHandler IndexCacheInitializing;
        event EventHandler IndexCacheRefreshed;
        event EventHandler IndexCacheRefreshing;
        event EventHandler IndexCacheSettingsRefreshed;
        event EventHandler IndexCacheSettingsRefreshing;

        Index AppendIndexMeta(string indexName, Dictionary<string, object> meta);
        Task<Index> AppendIndexMetaAsync(string indexName, Dictionary<string, object> meta);
        bool CanPing();
        Task<bool> CanPingAsync();
        bool Commit(BaseDocument document);
        bool Commit(IEnumerable<object> documents);
        bool Commit<T>(IEnumerable<T> documents) where T : class, IDocument;
        bool Commit<T>(T document) where T : class, IDocument;
        Task<bool> CommitAsync(IEnumerable<object> documents);
        Task<bool> CommitAsync(object document);
        Task<bool> CommitAsync<T>(IEnumerable<T> documents) where T : class, IDocument;
        Task<bool> CommitAsync<T>(T document) where T : class, IDocument;
        Task<Index> CopyIndexAsync(string sourceIndex, string targetIndex, bool includeDocs = false);
        Index CreateIndex(IndexConfig config);
        Task<Index> CreateIndexAsync(IndexConfig config);
        bool Delete(BaseDocument document);
        bool Delete(IEnumerable<BaseDocument> documents);
        bool Delete<T>(IEnumerable<T> documents) where T : BaseDocument;
        bool Delete<T>(T document) where T : BaseDocument;
        Task<bool> DeleteAsync(IEnumerable<object> documents);
        Task<bool> DeleteAsync(object document);
        Task<bool> DeleteAsync<T>(IEnumerable<T> documents) where T : BaseDocument;
        Task<bool> DeleteAsync<T>(T document) where T : BaseDocument;
        bool DeleteIndex(string indexName, bool bypassCache = false, bool preserveDependentIndices = false);
        Task<bool> DeleteIndexAsync(string indexName, bool bypassCache = false, bool preserveDependentIndices = false);
        Index DeleteIndexMeta(string indexName);
        Task<Index> DeleteIndexMetaAsync(string indexName);
        Index DeprecateIndex(string indexName, string deprecationMessage);
        Task<Index> DeprecateIndexAsync(string indexName, string deprecationMessage);
        Index DoNotForceRefreshOnDocumentCommit(string indexName);
        Task<Index> DoNotForceRefreshOnDocumentCommitAsync(string indexName);
        Index ExcludeIndexFromGlobalSearch(string indexName);
        Task<Index> ExcludeIndexFromGlobalSearchAsync(string indexName);
        Index ForceRefreshOnDocumentCommit(string indexName);
        Task<Index> ForceRefreshOnDocumentCommitAsync(string indexName);
        Index GetIndexDefinition(string indexName);
        Task<Index> GetIndexDefinitionAsync(string indexName);
        Index HideIndex(string indexName);
        Task<Index> HideIndexAsync(string indexName);
        Index IncludeIndexInGlobalSearch(string indexName);
        Task<Index> IncludeIndexInGlobalSearchAsync(string indexName);
        Task InitializeIndexCache();
        ISeaqQueryResults Query(ISeaqQuery query);
        ISeaqQueryResults<T> Query<T>(ISeaqQuery<T> query) where T : BaseDocument;
        TResp Query<TResp>(ISeaqQuery query) where TResp : class, ISeaqQueryResults;
        Task<ISeaqQueryResults> QueryAsync(ISeaqQuery query);
        Task<ISeaqQueryResults<T>> QueryAsync<T>(ISeaqQuery<T> query) where T : BaseDocument;
        Task<TResp> QueryAsync<TResp>(ISeaqQuery query) where TResp : class, ISeaqQueryResults;
        Task RefreshIndexCache();
        Task RefreshIndexSettings();
        Index ReplaceIndexMeta(string indexName, Dictionary<string, object> meta);
        Task<Index> ReplaceIndexMetaAsync(string indexName, Dictionary<string, object> meta);
        Index SetIndexMeta(string indexName, Dictionary<string, object> meta);
        Task<Index> SetIndexMetaAsync(string indexName, Dictionary<string, object> meta);
        Index SetIndexObjectLabel(string indexName, string label);
        Task<Index> SetIndexObjectLabelAsync(string indexName, string label);
        Index SetIndexObjectLabelPlural(string indexName, string label);
        Task<Index> SetIndexObjectLabelPluralAsync(string indexName, string label);
        Index SetIndexPrimaryField(string indexName, string fieldName);
        Task<Index> SetIndexPrimaryFieldAsync(string indexName, string fieldName);
        Index SetIndexPrimaryFieldLabel(string indexName, string label);
        Task<Index> SetIndexPrimaryFieldLabelAsync(string indexName, string label);
        Index SetIndexSecondaryField(string indexName, string fieldName);
        Task<Index> SetIndexSecondaryFieldAsync(string indexName, string fieldName);
        Index SetIndexSecondaryFieldLabel(string indexName, string label);
        Task<Index> SetIndexSecondaryFieldLabelAsync(string indexName, string label);
        Index UnDeprecateIndex(string indexName);
        Task<Index> UnDeprecateIndexAsync(string indexName);
        Index UnHideIndex(string indexName);
        Task<Index> UnHideIndexAsync(string indexName);
        Index UpdateIndexDefinition(Index index);
        Task<Index> UpdateIndexDefinitionAsync(Index index);
        Index UpdateIndexField(string indexName, Field field);
        Task<Index> UpdateIndexFieldAsync(string indexName, Field field);
    }
}