using System;
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

        /// <summary>
        /// Additional information about this query execution
        /// </summary>
        public IEnumerable<string> Messages { get; set; } = Array.Empty<string>();

        public SimpleQueryResults() { }
        public SimpleQueryResults(
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
        public SimpleQueryResults(
            Nest.ISearchResponse<BaseDocument> searchResponse,
            IEnumerable<string> messages = null)
        {
            Results = searchResponse.Hits.Select(x => new DefaultQueryResult(x));

            Took = searchResponse.Took;
            Total = searchResponse.Total;
            Messages = messages;
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

        /// <summary>
        /// Additional information about this query execution
        /// </summary>
        public IEnumerable<string> Messages { get; set; } = Array.Empty<string>();

        public SimpleQueryResults() { }
        public SimpleQueryResults(
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
        public SimpleQueryResults(
            Nest.ISearchResponse<T> searchResponse,
            IEnumerable<string> messages = null)
        {
            Results = searchResponse.Hits.Select(x => new DefaultQueryResult<T>(x));

            Took = searchResponse.Took;
            Total = searchResponse.Total;
            Messages = messages;
        }
    }
}
