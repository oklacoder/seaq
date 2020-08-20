namespace Seaq.Elasticsearch.Clusters
{
    public class ClusterProps
    {
        public string ScopeId { get; }
        public bool ForceRefreshOnCommit { get; }
        public bool EagerlyPersistStoreMetaDefault { get; }

        public ClusterProps(
            string scopeId,
            bool forceRefreshOnCommit,
            bool eagerlyPersistStoreMetaDefault)
        {
            ScopeId = scopeId;
            ForceRefreshOnCommit = forceRefreshOnCommit;
            EagerlyPersistStoreMetaDefault = eagerlyPersistStoreMetaDefault;
        }
    }
}
