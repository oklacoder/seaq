namespace seaq
{
    public interface IPercentileResult
    {
        public string Field { get; }
        public double? Percentile { get; }
        public double? Value { get; }
    }
}
