using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public interface ISeaqQueryCriteria 
    {
        public string Type { get; }
        Nest.SearchDescriptor<BaseDocument> GetSearchDescriptor();
        void ApplyClusterSettings(Cluster cluster);
        public string[] Indices { get; }
        public int? Skip { get; }
        public int? Take { get; }
        IEnumerable<ISortField> SortFields { get; }
        IEnumerable<IReturnField> ReturnFields { get; }
        IEnumerable<IBucketField> BucketFields { get; }
        IEnumerable<string> BoostedFields { get; }
    }
    public interface ISeaqQueryCriteria<T>
        where T : BaseDocument
    {
        Nest.SearchDescriptor<T> GetSearchDescriptor();
        void ApplyClusterSettings(Cluster cluster);
        public string[] Indices { get; }
        public int? Skip { get; }
        public int? Take { get; }
        IEnumerable<ISortField> SortFields { get; }
        IEnumerable<IReturnField> ReturnFields { get; }
        IEnumerable<IBucketField> BucketFields { get; }
        IEnumerable<string> BoostedFields { get; }
    }
}
