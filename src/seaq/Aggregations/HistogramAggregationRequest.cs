using Nest;

namespace seaq
{
    public class HistogramAggregationRequest :
        DefaultAggregationRequest
    {
        private readonly double? interval;
        private readonly double? offset;
        private readonly int? minBucketSize;

        public HistogramAggregationRequest(
            DefaultAggregationField field,
            double? interval = null,
            double? offset = null,
            int? minBucketSize = null)
            : base(DefaultAggregationCache.DateHistogramAggregation.Name, field)
        {
            this.interval = interval;
            this.offset = offset;
            this.minBucketSize = minBucketSize;
        }

        public override AggregationContainerDescriptor<T> ApplyAggregationDescriptor<T>(
            AggregationContainerDescriptor<T> agg,
            IAggregationCache aggregationCache)
        {

            return new HistogramAggregation(interval, offset, minBucketSize).ApplyAggregationDescriptor<T>(agg, Field);
        }
        public override AggregationContainerDescriptor<T> GetAggregationDescriptor<T>(
            IAggregationCache aggregationCache)
        {
            return new HistogramAggregation(interval, offset, minBucketSize).GetAggregationDescriptor<T>(Field);
        }
    }

}
