using System;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class AggregationQueryResults :
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

        public IEnumerable<IAggregationResult> AggregationResults { get; }

        public long Took { get; }

        public long Total { get; }

        /// <summary>
        /// Additional information about this query execution
        /// </summary>
        public IEnumerable<string> Messages { get; set; } = Array.Empty<string>();

        public AggregationQueryResults(
            Nest.ISearchResponse<BaseDocument> searchResponse,
            AggregationQueryCriteria criteria,
            IEnumerable<string> messages = null)
        {
            if (searchResponse.IsValid)
            {
                Results = Array.Empty<DefaultQueryResult>();

                AggregationResults = QueryHelper.BuildAggregationsResult(searchResponse.Aggregations, criteria);

                Total = searchResponse.Total;
                Took = searchResponse.Took;

                Messages = messages;
            }
            else
            {
                Results = null;
                AggregationResults = null;
                Total = searchResponse.Total;
                Took = searchResponse.Took;

                Messages = new[] { searchResponse?.OriginalException.Message };
            }

        }
    }
    public class AggregationQueryResults<T> :
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

        public IEnumerable<IAggregationResult> AggregationResults { get; }

        public long Took { get; }

        public long Total { get; }

        /// <summary>
        /// Additional information about this query execution
        /// </summary>
        public IEnumerable<string> Messages { get; set; } = Array.Empty<string>();

        public AggregationQueryResults(
            Nest.ISearchResponse<T> searchResponse,
            AggregationQueryCriteria<T> criteria,
            IEnumerable<string> messages = null)
        {
            if (searchResponse.IsValid)
            {
                Results = Array.Empty<DefaultQueryResult<T>>();

                AggregationResults = QueryHelper.BuildAggregationsResult(searchResponse.Aggregations, criteria);

                Total = searchResponse.Total;
                Took = searchResponse.Took;

                Messages = messages;
            }
            else
            {
                Results = null;
                AggregationResults = null;
                Total = searchResponse.Total;
                Took = searchResponse.Took;

                Messages = new[] { searchResponse?.OriginalException.Message };
            }

        }
    }
}
