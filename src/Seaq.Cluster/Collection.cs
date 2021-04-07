using System;

namespace Seaq.Clusters{
    public class Collection :
        ICollection
    {
        public string CollectionName { get; private set; }
        public Type DocumentType { get; private set; }
        public int PrimaryShards { get; private set; }
        public int ReplicaShards { get; private set; }
        public bool ForceRefreshOnDocumentCommit { get; private set; }
        public bool EagerlyPersistSchema { get; private set; }

        private CollectionSchema _schema { get; set; }

        public ICollectionSchema Schema => _schema;

        public Collection(
            ICollectionConfig config)
        {
            var _config = config as CollectionConfig;
            if (_config == null)
                throw new ArgumentException();
            ApplyConfig(_config);
        }

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
            _schema = config._schema;
        }

        public Collection(
            CollectionSchema schema)
        {
            CollectionName = schema.CollectionName;
            DocumentType = FieldNameUtilities.GetSearchableType(schema.CollectionDocumentType);
            _schema = schema;
        }

        public void SetSchema(ICollectionSchema schema)
        {
            var _schema = schema as CollectionSchema;

            if (_schema == null)
                throw new ArgumentException();

            this._schema = _schema;
        }

        public void SetSchema(CollectionSchema schema)
        {
            this._schema = schema;
        }
    }

}
