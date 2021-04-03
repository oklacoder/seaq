using System.Collections.Generic;

namespace Seaq.Clusters{
    public interface ICollectionSchema
    {
        string CollectionName { get; }
        string CollectionDocumentType { get; }
        IEnumerable<ICollectionField> Fields { get; }
        IEnumerable<string> GetFilterableFieldNames(params string[] fieldNames);
        IEnumerable<string> GetBoostedFieldNames(params string[] fieldNames);
        IEnumerable<string> GetIncludedFieldNames(params string[] fieldNames);
        IEnumerable<string> GetKeywordFieldNames(params string[] fieldNames);
        IEnumerable<string> GetSortFieldNames(params string[] fieldNames);
        void AddField(CollectionField field);
    }

}
