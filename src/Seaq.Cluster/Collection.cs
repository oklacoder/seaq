using System;

namespace Seaq.Clusters{
    public class Collection :
        ICollection
    {
        public string CollectionName { get; }

        public Type DocumentType { get; }

        public int PrimaryShards { get; }
        public int ReplicaShards { get; }
        public bool ForceRefreshOnDocumentCommit { get; }
        public bool EagerlyPersistSchema { get; }

        public ICollectionSchema Schema { get; private set; }

        public Collection(
            ICollectionConfig config)
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
            ICollectionSchema schema)
        {
            CollectionName = schema.CollectionName;
            DocumentType = FieldNameUtilities.GetSearchableType(schema.CollectionDocumentType);
            Schema = schema;
        }

        public void SetSchema(ICollectionSchema schema)
        {
            this.Schema = schema;
        }
    }

}
