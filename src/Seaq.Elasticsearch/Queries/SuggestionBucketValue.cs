namespace Seaq.Elasticsearch.Queries
{
    public class SuggestionBucketValue :
        IBucketValue
    {
        public string Key { get; }
        public int Count { get; }

        public SuggestionBucketValue(
            string key,
            int count)
        {
            Key = key;
            Count = count;
        }
    }
}