using Nest;
using Seaq.Elasticsearch.Queries;

namespace Seaq.Elasticsearch.Extensions
{
    public static class BucketExtension
    {
        public static SuggestionBucketValue BuildSuggestionBucket(
            this KeyedBucket<string> bucket)
        {
            return new SuggestionBucketValue(bucket.Key, (int)bucket.DocCount);
        }
    }
}
