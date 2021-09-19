using System.Collections.Generic;

namespace seaq
{
    public class SimpleQueryResults<T> :
        ISeaqQueryResults<T>
        where T : class, IDocument
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
