using Seaq.Elasticsearch.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Seaq.Elasticsearch.Queries
{
    public class FieldValuesQueryResult :
        IQueryResult
    {
        private Paging _paging { get; }
        public IResultMeta ResultMeta { get; }

        public IPaging Paging => _paging;

        public ImmutableList<IDocument> Results { get; }
        private ImmutableList<SuggestionBucket> _buckets { get; }
        public ImmutableList<IBucket> Buckets => ImmutableList<IBucket>.Empty.AddRange(_buckets);

        public FieldValuesQueryResult(
            SuggestionBucket[] buckets,
            Paging paging,
            IDocument[] results,
            IResultMeta resultMeta)
        {
            _buckets = ImmutableList<SuggestionBucket>.Empty.AddRange(buckets);
            _paging = paging;
            ResultMeta = resultMeta;
            Results = ImmutableList<IDocument>.Empty.AddRange(results);
        }

    }
}
