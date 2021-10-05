using System.Collections.Generic;

namespace seaq
{
    public class SimpleQueryResults :
        ISeaqQueryResults
    {
        public IEnumerable<BaseDocument> Documents { get; }

        public long Took { get; }

        public long Total { get; }

        public SimpleQueryResults() { }
        public SimpleQueryResults(
            IEnumerable<BaseDocument> documents,
            long took,
            long total)
        {
            Documents = documents;
            Took = took;
            Total = total;
        }
        public SimpleQueryResults(
            Nest.ISearchResponse<BaseDocument> searchResponse)
        {
            Documents = searchResponse.Documents;
            Took = searchResponse.Took;
            Total = searchResponse.Total;
        }
    }
    public class SimpleQueryResults<T> :
        ISeaqQueryResults<T>
        where T : BaseDocument
    {
        public IEnumerable<T> Documents { get; }

        public long Took { get; }

        public long Total { get; }

        public SimpleQueryResults() { }
        public SimpleQueryResults(
            IEnumerable<T> documents,
            long took,
            long total)
        {
            Documents = documents;
            Took = took;
            Total = total;
        }
        public SimpleQueryResults(
            Nest.ISearchResponse<T> searchResponse)
        {
            Documents = searchResponse.Documents;
            Took = searchResponse.Took;
            Total = searchResponse.Total;
        }
    }
}
