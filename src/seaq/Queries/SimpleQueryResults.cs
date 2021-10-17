using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class SimpleQueryResults :
        ISeaqQueryResults
    {
        public IEnumerable<DefaultQueryResult> Results { get; }
        IEnumerable<ISeaqQueryResult> ISeaqQueryResults.Results => Results;

        public long Took { get; }

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
        public IEnumerable<DefaultQueryResult<T>> Results { get; }
        IEnumerable<ISeaqQueryResult<T>> ISeaqQueryResults<T>.Results => Results;

        public long Took { get; }

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
