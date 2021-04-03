using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seaq
{
    public class AdvancedQueryResults<T> :
        IQueryResults<T>
        where T : class, IDocument
    {
        public IEnumerable<T> Documents { get; }
        public IEnumerable<IBucketResult> Buckets { get; }
        public long Total { get; }
        public long Took { get; }

        public AdvancedQueryResults(){}
        public AdvancedQueryResults(
            IEnumerable<T> documents,
            long took,
            long total)
        {
            Documents = documents;
            Took = took;
            Total = total;
        }
        public AdvancedQueryResults(
            Nest.ISearchResponse<T> searchResponse)
        {
            Documents = searchResponse.Documents;
            Total = searchResponse.Total;
            Took = searchResponse.Took;
            Buckets = searchResponse.Aggregations?.BuildBucketResult();
        }
    }

}
