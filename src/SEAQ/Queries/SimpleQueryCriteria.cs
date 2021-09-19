using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class SimpleQueryCriteria<T> :
        ISeaqQueryCriteria<T>
        where T : class, IDocument
    {
        public string Text { get; }
        public string[] Indices { get; private set; }

        public int? Skip { get; }
        public int? Take { get; }

        private IEnumerable<DefaultSortField> _sortFields { get; }
        public IEnumerable<ISortField> SortFields => _sortFields;
        public IEnumerable<IReturnField> ReturnFields { get; }
        public IEnumerable<IBucketField> BucketFields { get; }

        public void ApplyClusterIndices(ILookup<string, Index> indices)
        {
            var idx = indices[typeof(T).FullName];
            var resp = Indices.ToList();
            resp.AddRange(
                idx
                    .Where(x => 
                        resp.Any(z => z
                            .Equals(x.Name, StringComparison.OrdinalIgnoreCase)))
                    .Select(x => x.Name));
            Indices = resp.ToArray();
        }

        public SimpleQueryCriteria() { }
        public SimpleQueryCriteria(
            string text,
            string[] indices = null,
            int? skip = null,
            int? take = null,
            IEnumerable<DefaultSortField> sortFields = null,
            IEnumerable<IBucketField> bucketFields = null,
            IEnumerable<IReturnField> returnFields = null)
        {
            Text = text;
            Indices = indices ?? Array.Empty<string>();
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
