using System.Collections.Immutable;
using Seaq.Elasticsearch.Documents;

namespace Seaq.Elasticsearch.Queries
{
    public class GetByIdsQueryResult :
        IQueryResult
    {
        public IPaging Paging { get; }

        public ImmutableList<IDocument> Results { get; }

        public IResultMeta ResultMeta { get; }

        public GetByIdsQueryResult(
            Paging paging,
            IDocument[] results,
            IResultMeta resultMeta)
        {
            Paging = paging;
            ResultMeta = resultMeta;
            Results = ImmutableList<IDocument>.Empty.AddRange(results);
        }
    }
}
