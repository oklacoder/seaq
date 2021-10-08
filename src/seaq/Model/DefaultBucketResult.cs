namespace seaq
{
    public class DefaultBucketResult :
        IBucketResult
    {
        public string Value { get; }
        public long? Count { get; }

        public DefaultBucketResult()
        {

        }
        public DefaultBucketResult(
            string value,
            long? count)
        {
            Value = value;
            Count = count;
        }

    }
}
