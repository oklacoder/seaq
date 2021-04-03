using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seaq
{
    public class AdvancedQueryCriteria<T> :
        IQueryCriteria<T>
        where T : class, IDocument
    {
        public string[] Indices { get; }

        public int? Skip { get; }
        public int? Take { get; }

        public IEnumerable<ISortField> SortFields { get; }
        public IEnumerable<IReturnField> ReturnFields { get; }
        public IEnumerable<IFilterField> FilterFields { get; }
        public IEnumerable<IBucketField> BucketFields { get; }

        public AdvancedQueryCriteria(
            string[] indices,
            IEnumerable<ISortField> sortFields = null,
            IEnumerable<IReturnField> returnFields = null,
            IEnumerable<IFilterField> filterFields = null,
            IEnumerable<IBucketField> bucketFields = null,
            int? skip = null,
            int? take = null)
        {
            Indices = indices;
            SortFields = sortFields;
            ReturnFields = returnFields;
            FilterFields = filterFields;
            BucketFields = bucketFields;
            Skip = skip;
            Take = take;
        }

        public SearchDescriptor<T> GetSearchDescriptor()
        {
            var s = new SearchDescriptor<T>()
                .Index(Indices)
                .Skip(Skip ?? 0)
                .Take(Take ?? 10)
                .Aggregations(a => BucketFields.GetBucketAggreagationDescriptor<T>())
                .Query(q => FilterFields.GetQueryDesctiptor<T>())
                .Source(t => ReturnFields.GetSourceFilterDescriptor<T>())
                .Sort(s => SortFields.GetSortDescriptor<T>());

            return s;
        }
    }
}
