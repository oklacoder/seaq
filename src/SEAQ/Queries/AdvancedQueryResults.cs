using System.Collections.Generic;

namespace seaq
{
    public class AdvancedQueryResults :
     AdvancedQueryResults<IDocument>
    {
        
        public AdvancedQueryResults() { }
        public AdvancedQueryResults(
            IEnumerable<BaseDocument> documents,
            long took,
            long total)
            : base(documents, took, total)
        {
        }
        public AdvancedQueryResults(
            Nest.ISearchResponse<BaseDocument> searchResponse)
            : base(searchResponse)
        {
        }
    }

    public class AdvancedQueryResults<T> :
        ISeaqQueryResults<T>
    where T : class, IDocument
    {
        public IEnumerable<T> Documents { get; }
        public IEnumerable<IBucketResult> Buckets { get; }
        public long Total { get; }
        public long Took { get; }

        public AdvancedQueryResults() { }
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
