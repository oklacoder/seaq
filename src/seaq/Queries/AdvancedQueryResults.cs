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

        /// <summary>
        /// Additional information about this query execution
        /// </summary>
        public IEnumerable<string> Messages { get; set; } = Enumerable.Empty<string>();

        public AdvancedQueryResults() { }
        public AdvancedQueryResults(
            IEnumerable<DefaultQueryResult> results,
            long took,
            long total,
            IEnumerable<string> messages = null)
        {
            Results = results;
            Took = took;
            Total = total;
            Messages = messages;
        }
        public AdvancedQueryResults(
            Nest.ISearchResponse<BaseDocument> searchResponse,
            IEnumerable<string> messages = null)
        {
            Results = searchResponse.Hits.Select(x => new DefaultQueryResult(x));

            Total = searchResponse.Total;
            Took = searchResponse.Took;
            Buckets = searchResponse.Aggregations?.BuildBucketResult();
            Messages = messages;
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

        /// <summary>
        /// Additional information about this query execution
        /// </summary>
        public IEnumerable<string> Messages { get; set; } = Enumerable.Empty<string>();

        public AdvancedQueryResults() { }
        public AdvancedQueryResults(
            IEnumerable<DefaultQueryResult<T>> results,
            long took,
            long total,
            IEnumerable<string> messages = null)
        {
            Results = results;
            Took = took;
            Total = total;
            Messages = messages;
        }
        public AdvancedQueryResults(
            Nest.ISearchResponse<T> searchResponse,
            IEnumerable<string> messages = null)
        {
            Results = searchResponse.Hits.Select(x => new DefaultQueryResult<T>(x));

            Total = searchResponse.Total;
            Took = searchResponse.Took;
            Buckets = searchResponse.Aggregations?.BuildBucketResult();
            Messages = messages;
        }
    }
}
