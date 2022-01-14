using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class SimpleQueryResults :
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
        /// Query execution duration
        /// </summary>
        public long Took { get; }

        /// <summary>
        /// Total query results before paging applied
        /// </summary>
        public long Total { get; }

        public SimpleQueryResults() { }
        public SimpleQueryResults(
            IEnumerable<DefaultQueryResult> results,
            long took,
            long total)
        {
            Results = results;
            Took = took;
            Total = total;
        }
        public SimpleQueryResults(
            Nest.ISearchResponse<BaseDocument> searchResponse)
        {
            Results = searchResponse.Hits.Select(x => new DefaultQueryResult(x));

            Took = searchResponse.Took;
            Total = searchResponse.Total;
        }
    }
    public class SimpleQueryResults<T> :
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
        /// Query execution duration
        /// </summary>
        public long Took { get; }

        /// <summary>
        /// Total query results before paging applied
        /// </summary>
        public long Total { get; }

        public SimpleQueryResults() { }
        public SimpleQueryResults(
            IEnumerable<DefaultQueryResult<T>> results,
            long took,
            long total)
        {
            Results = results;
            Took = took;
            Total = total;
        }
        public SimpleQueryResults(
            Nest.ISearchResponse<T> searchResponse)
        {
            Results = searchResponse.Hits.Select(x => new DefaultQueryResult<T>(x));

            Took = searchResponse.Took;
            Total = searchResponse.Total;
        }
    }
}
