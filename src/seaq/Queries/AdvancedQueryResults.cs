using System.Collections.Generic;

namespace seaq
{
    public class AdvancedQueryResults :
        ISeaqQueryResults
    {

        public IEnumerable<BaseDocument> Documents { get; }
        public IEnumerable<IBucketResult> Buckets { get; }
        public long Total { get; }
        public long Took { get; }
        public AdvancedQueryResults() { }
        public AdvancedQueryResults(
            IEnumerable<BaseDocument> documents,
            long took,
            long total)
        {
            Documents = documents;
            Took = took;
            Total = total;
        }
        public AdvancedQueryResults(
            Nest.ISearchResponse<BaseDocument> searchResponse)
        {
            //want to capture the scores, so need to pull from hits rather than docs directly.  probably with a poco here that contains a "document" prop
            Documents = searchResponse.Documents;
            Total = searchResponse.Total;
            Took = searchResponse.Took;
            Buckets = searchResponse.Aggregations?.BuildBucketResult();
        }
    }

    public class AdvancedQueryResults<T> :
        ISeaqQueryResults<T>
    where T : BaseDocument
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
