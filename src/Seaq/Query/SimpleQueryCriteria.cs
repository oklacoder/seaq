using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seaq
{
    public class SimpleQueryCriteria<T> :
        IQueryCriteria<T>
        where T : class, IDocument
    {
        public string Text { get; }
        public string[] Indices { get; }

        public int? Skip { get; }
        public int? Take { get; }

        private IEnumerable<DefaultSortField> _sortFields { get; }
        public IEnumerable<ISortField> SortFields => _sortFields;
        public IEnumerable<IReturnField> ReturnFields { get; }
        public IEnumerable<IBucketField> BucketFields { get; }

        public SimpleQueryCriteria() { }
        public SimpleQueryCriteria(
            string text,
            string[] indices,
            int? skip = null,
            int? take = null,
            IEnumerable<DefaultSortField> sortFields = null,
            IEnumerable<IBucketField> bucketFields = null,
            IEnumerable<IReturnField> returnFields = null)
        {
            Text = text;
            Indices = indices;
            Skip = skip;
            Take = take;
            _sortFields = sortFields ?? Array.Empty<DefaultSortField>();
            BucketFields = bucketFields;
            ReturnFields = returnFields;
        }

        public SearchDescriptor<T> GetSearchDescriptor()
        {
            var res = new SearchDescriptor<T>()
                .Index(Indices)
                .Skip(Skip ?? 0)
                .Take(Take ?? 10)
                .Aggregations(a => BucketFields.GetBucketAggreagationDescriptor<T>())
                .Sort(x => SortFields.GetSortDescriptor<T>())
                .Query(x => x.QueryString(q => q.Query(Text)));


            return res;
        }
    }
}
