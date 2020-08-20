using Seaq.Elasticsearch.Documents;
using Seaq.Elasticsearch.Queries;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Seaq.Elasticsearch.Queries
{
    public class SimpleQueryResult :
        IQueryResult
    {
        public IPaging Paging { get; }

        public ImmutableList<IDocument> Results { get; }

        public IResultMeta ResultMeta { get; }

        public SimpleQueryResult(
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
