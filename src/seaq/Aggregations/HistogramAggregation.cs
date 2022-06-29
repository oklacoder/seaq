using Nest;
using System;
using System.Linq;

namespace seaq
{
    public class HistogramAggregation :
        BaseAggregation
    {
        public double? Interval { get; set; } = 1;
        public double? Offset { get; set; } = 0;
        public int? MinBucketSize { get; set; } = 1;

        public HistogramAggregation()
        {

        }
        public HistogramAggregation(
            double? interval,
            double? offset,
            int? minBucketSize)
        {
            if (interval.HasValue)
                Interval = interval;
            if (offset.HasValue)
                Offset = offset;
            if (minBucketSize.HasValue)
                MinBucketSize = minBucketSize;
        }

        public override AggregationContainerDescriptor<T> ApplyAggregationDescriptor<T>(AggregationContainerDescriptor<T> agg, IAggregationField field)
        {
            if (string.IsNullOrWhiteSpace(field?.FieldName))
                return agg;

            var key = Constants.Fields.NestReservedFieldNames
                .Contains(field.FieldName, StringComparer.OrdinalIgnoreCase) ?
                    $"@{field.FieldName}" :
                    field.FieldName;

            key = $"{Name}{Constants.TextPartSeparator}{key}";

            agg.Histogram(key, t => t
                .Field(field.FieldName)
                .Interval(Interval)
                .Offset(Offset)
                .MinimumDocumentCount(MinBucketSize));

            return agg;
        }

        public override AggregationContainerDescriptor<T> GetAggregationDescriptor<T>(IAggregationField field)
        {
            var res = new AggregationContainerDescriptor<T>();

            ApplyAggregationDescriptor(res, field);

            return res;
        }

        public override IAggregationResult BuildAggregationResult(AggregateDictionary aggs, string aggKey, string fieldName)
        {
            return new HistogramAggregationResult(aggs, aggKey, fieldName);
        }
    }

}
