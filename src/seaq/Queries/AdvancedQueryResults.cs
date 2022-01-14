using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class AdvancedQueryResults :
        ISeaqQueryResults
    {

        /// <summary>
        /// Collection of query result objects
        /// </summary>
        public IEnumerable<DefaultQueryResult> Results { get; }
        /// <summary>
        /// Collection of query result objects
        /// </summary>
        IEnumerable<ISeaqQueryResult> ISeaqQueryResults.Results => Results;
        /// <summary>
        /// Collection of aggregation bucket objects
        /// </summary>
        public IEnumerable<IBucketResult> Buckets { get; }
        /// <summary>
        /// Query execution duration
        /// </summary>
        public long Total { get; }
        /// <summary>
        /// Total query results before paging applied
        /// </summary>
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
        /// <summary>
        /// Collection of query result objects
        /// </summary>
        public IEnumerable<DefaultQueryResult<T>> Results { get; }
        /// <summary>
        /// Collection of query result objects
        /// </summary>
        IEnumerable<ISeaqQueryResult<T>> ISeaqQueryResults<T>.Results => Results;
        /// <summary>
        /// Collection of aggregation bucket objects
        /// </summary>
        public IEnumerable<IBucketResult> Buckets { get; }
        /// <summary>
        /// Query execution duration
        /// </summary>
        public long Total { get; }
        /// <summary>
        /// Total query results before paging applied
        /// </summary>
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
