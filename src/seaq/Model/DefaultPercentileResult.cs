namespace seaq
{
    public class DefaultPercentileResult :
        IPercentileResult
    {
        public string Field { get; set; }
        public double? Percentile { get; set; }
        public double? Value { get; set; }

        public DefaultPercentileResult(
            string field,
            double? percentile,
            double? value)
        {
            Field = field;
            Percentile = percentile;
            Value = value;
        }

    }
}
