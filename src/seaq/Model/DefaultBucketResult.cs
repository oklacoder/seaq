namespace seaq
{
    public class DefaultBucketResult :
        IBucketResult
    {
        public string Key { get; }
        public string Value { get; }
        public long? Count { get; }

        public DefaultBucketResult()
        {

        }
        public DefaultBucketResult(
            string key,
            string value,
            long? count)
        {
            Key = key;
            Value = value;
            Count = count;
        }

    }
}
