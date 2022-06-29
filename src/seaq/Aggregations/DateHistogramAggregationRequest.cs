using Nest;

namespace seaq
{
    public class DateHistogramAggregationRequest :
        DefaultAggregationRequest
    {
        private readonly string interval;
        private readonly string offset;
        private readonly int? minBucketSize;

        public DateHistogramAggregationRequest(
            DefaultAggregationField field,
            string interval = null,
            string offset = null,
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

            return new DateHistogramAggregation(interval, offset, minBucketSize).ApplyAggregationDescriptor<T>(agg, Field);
        }
        public override AggregationContainerDescriptor<T> GetAggregationDescriptor<T>(
            IAggregationCache aggregationCache)
        {
            return new DateHistogramAggregation(interval, offset, minBucketSize).GetAggregationDescriptor<T>(Field);
        }
    }

}
