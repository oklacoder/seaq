namespace seaq
{
    public class IndexConfig
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int PrimaryShards { get; set; }
        public int ReplicaShards { get; set; }
        public bool ForceRefreshOnDocumentCommit { get; set; }
        public bool EagerlyPersistSchema { get; set; }

        public IndexConfig(
            string name,
            string type,
            int? primaryShards = null,
            int? replicaShards = null,
            bool? forceRefreshOnDocumentCommit = null,
            bool? eagerlyPersistSchema = null)
        {

            Name = name;
            Type = type;
            PrimaryShards = primaryShards ?? Constants.Indices.Defaults.PrimaryShardsDefault;
            ReplicaShards = replicaShards ?? Constants.Indices.Defaults.ReplicaShardsDefault;
            ForceRefreshOnDocumentCommit = forceRefreshOnDocumentCommit ?? Constants.Indices.Defaults.ForceRefreshOnDocumentCommitDefault;
            EagerlyPersistSchema = eagerlyPersistSchema ?? Constants.Indices.Defaults.EagerlyPersistSchema;
        }

        public IndexConfig(
            Index index)
        {
            Name = index.Name;
            Type = index.Type;
            PrimaryShards = index.PrimaryShards;
            ReplicaShards = index.ReplicaShards;
            ForceRefreshOnDocumentCommit = index.ForceRefreshOnDocumentCommit;
            EagerlyPersistSchema = index.EagerlyPersistSchema;
        }
    }
}
