using System;

namespace Seaq.Clusters{
    public class Collection
    {
        public string CollectionName { get; private set; }
        public Type DocumentType { get; private set; }
        public int PrimaryShards { get; private set; }
        public int ReplicaShards { get; private set; }
        public bool ForceRefreshOnDocumentCommit { get; private set; }
        public bool EagerlyPersistSchema { get; private set; }

        public CollectionSchema Schema { get; set; }
        

        public Collection(
            CollectionConfig config)
        {
            ApplyConfig(config);
        }

        private void ApplyConfig(CollectionConfig config)
        {
            CollectionName = config.Name;
            DocumentType = config.DocumentType;
            PrimaryShards = config.PrimaryShards;
            ReplicaShards = config.ReplicaShards;
            ForceRefreshOnDocumentCommit = config.ForceRefreshOnDocumentCommit;
            EagerlyPersistSchema = config.EagerlyPersistSchema;
            Schema = config.Schema;
        }

        public Collection(
            CollectionSchema schema)
        {
            CollectionName = schema.CollectionName;
            DocumentType = FieldNameUtilities.GetSearchableType(schema.CollectionDocumentType);
            Schema = schema;
        }

        public void SetSchema(CollectionSchema schema)
        {
            this.Schema = schema;
        }
    }

}
