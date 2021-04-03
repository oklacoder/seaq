using System;

namespace Seaq.Clusters{
    public interface ICollection
    {
        string CollectionName { get; }
        Type DocumentType { get; }
        ICollectionSchema Schema { get; }
        bool ForceRefreshOnDocumentCommit { get; }

        void SetSchema(ICollectionSchema schema);
    }

}
