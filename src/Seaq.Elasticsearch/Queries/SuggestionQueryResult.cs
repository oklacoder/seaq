using Nest;
using Seaq.Elasticsearch.Documents;
using System.Collections.Immutable;

namespace Seaq.Elasticsearch.Queries
{
    public class SuggestionQueryResult :
        IQueryResult
    {
        private ImmutableList<SuggestionBucket> _buckets { get; }
        public ImmutableList<IBucket> Buckets => ImmutableList<IBucket>.Empty.AddRange(_buckets);

        private Paging _paging { get; }
        public IPaging Paging => _paging;

        public ImmutableList<IDocument> Results { get; }

        public IResultMeta ResultMeta { get; }

        public SuggestionQueryResult(
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