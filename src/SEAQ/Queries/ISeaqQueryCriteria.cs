using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public interface ISeaqQueryCriteria<T>
        where T : BaseDocument
    {
        Nest.SearchDescriptor<T> GetSearchDescriptor();
        void ApplyClusterIndices(ILookup<string, Index> indices);
        public string[] Indices { get; }
        public int? Skip { get; }
        public int? Take { get; }
        IEnumerable<ISortField> SortFields { get; }
        IEnumerable<IReturnField> ReturnFields { get; }
        IEnumerable<IBucketField> BucketFields { get; }
    }
}
