namespace seaq
{
    public interface IBucketResult
    {
        public string Key { get; }
        public string Value { get; }
        public long? Count { get; }
    }
}
