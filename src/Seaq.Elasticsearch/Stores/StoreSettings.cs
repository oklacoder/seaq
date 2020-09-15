using System;

namespace Seaq.Elasticsearch.Stores
{
    public class CreateStoreSettings
    {
        const int _primaryShardCountDefault = 1;
        const int _replicaShardCountDefault = 2;
        const bool _eagerlyPersistStoreMetaDefault = true;

        public CreateStoreSettings(
            string moniker,
            string scopeId,
            string typeFullName,
            int primaryShards = _primaryShardCountDefault,
            int replicaShards = _replicaShardCountDefault,
            bool eagerlyPersistStoreMeta = _eagerlyPersistStoreMetaDefault)
        {
            Moniker = moniker;
            ScopeId = scopeId;
            TypeFullName = typeFullName;
            PrimaryShards = primaryShards;
            ReplicaShards = replicaShards;
            EagerlyPersistStoreMeta = eagerlyPersistStoreMeta;
        }

        public string Moniker { get; }
        public string ScopeId { get; }
        public string TypeFullName { get; }
        public int PrimaryShards { get; }
        public int ReplicaShards { get; }
        public bool EagerlyPersistStoreMeta { get; }
    }
}
