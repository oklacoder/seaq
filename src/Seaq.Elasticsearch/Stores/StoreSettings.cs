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
            Type type,
            int primaryShards = _primaryShardCountDefault,
            int replicaShards = _replicaShardCountDefault,
            bool eagerlyPersistStoreMeta = _eagerlyPersistStoreMetaDefault)
        {
            Moniker = moniker;
            ScopeId = scopeId;
            Type = type;
            PrimaryShards = primaryShards;
            ReplicaShards = replicaShards;
            EagerlyPersistStoreMeta = eagerlyPersistStoreMeta;
        }

        public string Moniker { get; }
        public string ScopeId { get; }
        public Type Type { get; }
        public int PrimaryShards { get; }
        public int ReplicaShards { get; }
        public bool EagerlyPersistStoreMeta { get; }
    }
}
