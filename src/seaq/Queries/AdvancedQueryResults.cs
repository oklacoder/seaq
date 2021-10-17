using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class AdvancedQueryResults :
        ISeaqQueryResults
    {

        public IEnumerable<DefaultQueryResult> Results { get; }
        IEnumerable<ISeaqQueryResult> ISeaqQueryResults.Results => Results;

        public IEnumerable<IBucketResult> Buckets { get; }
        public long Total { get; }
        public long Took { get; }
        public AdvancedQueryResults() { }
        public AdvancedQueryResults(
            IEnumerable<DefaultQueryResult> results,
            long took,
            long total)
        {
            Results = results;
            Took = took;
            Total = total;
        }
        public AdvancedQueryResults(
            Nest.ISearchResponse<BaseDocument> searchResponse)
        {
            Results = searchResponse.Hits.Select(x => new DefaultQueryResult(x));

            Total = searchResponse.Total;
            Took = searchResponse.Took;
            Buckets = searchResponse.Aggregations?.BuildBucketResult();
        }
    }

    public class AdvancedQueryResults<T> :
        ISeaqQueryResults<T>
    where T : BaseDocument
    {
        public IEnumerable<DefaultQueryResult<T>> Results { get; }
        IEnumerable<ISeaqQueryResult<T>> ISeaqQueryResults<T>.Results => Results;

        public IEnumerable<IBucketResult> Buckets { get; }
        public long Total { get; }
        public long Took { get; }

        public AdvancedQueryResults() { }
        public AdvancedQueryResults(
            IEnumerable<DefaultQueryResult<T>> results,
            long took,
            long total)
        {
            Results = results;
            Took = took;
            Total = total;
        }
        public AdvancedQueryResults(
            Nest.ISearchResponse<T> searchResponse)
        {
            Results = searchResponse.Hits.Select(x => new DefaultQueryResult<T>(x));

            Total = searchResponse.Total;
            Took = searchResponse.Took;
            Buckets = searchResponse.Aggregations?.BuildBucketResult();
        }
    }
}
