using Seaq.Elasticsearch.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Seaq.Elasticsearch.Queries
{
    public class DirectQueryResult :
        IQueryResult
    {
        public IPaging Paging { get; }

        public ImmutableList<IDocument> Results { get; }

        public IResultMeta ResultMeta { get; }

        public DirectQueryResult(
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
