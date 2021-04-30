using System;

namespace Seaq.Clusters{

    public class CollectionConfig 
    {
        const int _primaryShardDefault = 1;
        const int _replicaShardDefault = 2;
        const bool _forceRefreshOnCommit = false;
        const bool _eagerlyPersistDefault = true;

        public string Name { get; private set; }
        public Type DocumentType { get; }
        public int PrimaryShards { get; }
        public int ReplicaShards { get; }
        public bool ForceRefreshOnDocumentCommit { get; }
        public bool EagerlyPersistSchema { get; }

        internal CollectionSchema Schema;

        public CollectionConfig(
            string name,
            Type documentType,
            int primaryShards = _primaryShardDefault,
            int replicaShards = _replicaShardDefault,
            bool forceRefreshOnDocumentCommit = _forceRefreshOnCommit,
            bool eagerlyPersistSchema = _eagerlyPersistDefault, 
            CollectionSchema schema = null)
        {
            Name = name;
            DocumentType = documentType;
            PrimaryShards = primaryShards;
            ReplicaShards = replicaShards;
            ForceRefreshOnDocumentCommit = forceRefreshOnDocumentCommit;
            EagerlyPersistSchema = eagerlyPersistSchema;
            Schema = schema;
        }

        public void AddScopeToName(string scopeId)
        {
            if (!Name.StartsWith(scopeId))
                Name = $"{scopeId}{Constants.CollectionSettings.CollectionNamePartSeparator}{Name}";
        }
    }

}
