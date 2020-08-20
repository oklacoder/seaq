using System.Collections.Immutable;

namespace Seaq.Elasticsearch.Queries
{

    public class SuggestionBucket :
        IBucket
    {
        public SuggestionBucket(
            string key,
            SuggestionBucketValue[] values)
        {
            _values = ImmutableList<SuggestionBucketValue>.Empty.AddRange(values);
            Key = key;
        }

        public string Key { get; }

        private ImmutableList<SuggestionBucketValue> _values { get; }
        public ImmutableList<IBucketValue> Values => ImmutableList<IBucketValue>.Empty.AddRange(_values);
    }
}
