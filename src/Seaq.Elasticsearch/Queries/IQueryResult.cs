using Seaq.Elasticsearch.Documents;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Seaq.Elasticsearch.Queries
{
    public interface IQueryResult
    {
        IPaging Paging { get; }

        ImmutableList<IDocument> Results { get; }

        IResultMeta ResultMeta { get; }
    }
}
