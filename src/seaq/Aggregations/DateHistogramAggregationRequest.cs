using Nest;
using System;

namespace seaq
{
    public class DateHistogramAggregationRequest :
        DefaultAggregationRequest
    {
        private readonly string interval;
        private readonly string offset;
        private readonly int? minBucketSize;
        private readonly DateTime? ExtendedBoundsMin;
        private readonly DateTime? ExtendedBoundsMax;

        public DateHistogramAggregationRequest(
            DefaultAggregationField field,
            string interval = null,
            string offset = null,
            int? minBucketSize = null,
            DateTime? extendedBoundsMin = null,
            DateTime? extendedBoundsMax = null)
            : base(DefaultAggregationCache.DateHistogramAggregation.Name, field)
        {
            this.interval = interval;
            this.offset = offset;
            this.minBucketSize = minBucketSize;
            ExtendedBoundsMin = extendedBoundsMin;
            ExtendedBoundsMax = extendedBoundsMax;
        }

        public override AggregationContainerDescriptor<T> ApplyAggregationDescriptor<T>(
            AggregationContainerDescriptor<T> agg,
            IAggregationCache aggregationCache)
        {

            return new DateHistogramAggregation(interval, offset, minBucketSize, ExtendedBoundsMin, ExtendedBoundsMax).ApplyAggregationDescriptor<T>(agg, Field);
        }
        public override AggregationContainerDescriptor<T> GetAggregationDescriptor<T>(
            IAggregationCache aggregationCache)
        {
            return new DateHistogramAggregation(interval, offset, minBucketSize, ExtendedBoundsMin, ExtendedBoundsMax).GetAggregationDescriptor<T>(Field);
        }
    }

}
