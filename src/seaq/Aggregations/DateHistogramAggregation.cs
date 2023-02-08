using Nest;
using System;
using System.Linq;

namespace seaq
{
    public class DateHistogramAggregation :
        BaseAggregation
    {
        public string Interval { get; set; } = Constants.DateIntervals.Day;
        public string Offset { get; set; } = "0";
        public int MinBucketSize { get; set; } = 1;
        public DateTime? ExtendedBoundsMin { get; set; } = null;
        public DateTime? ExtendedBoundsMax { get; set; } = null;

        public DateHistogramAggregation()
        {

        }
        public DateHistogramAggregation(
            string? interval = null,
            string? offset = null,
            int? minBucketSize = null,
            DateTime? extendedBoundsMin = null,
            DateTime? extendedBoundsMax = null)
        {
            if (!string.IsNullOrWhiteSpace(interval))
                Interval = interval;
            if (!string.IsNullOrWhiteSpace(offset))
                Offset = offset;
            if (minBucketSize.HasValue)
                MinBucketSize = minBucketSize.Value;
            if (extendedBoundsMin.HasValue)
                ExtendedBoundsMin = extendedBoundsMin;
            if (extendedBoundsMax.HasValue)
                ExtendedBoundsMax = extendedBoundsMax;
        }

        public override AggregationContainerDescriptor<T> ApplyAggregationDescriptor<T>(
            AggregationContainerDescriptor<T> agg, 
            IAggregationField field)
        {
            if (string.IsNullOrWhiteSpace(field?.FieldName))
                return agg;

            var key = Constants.Fields.NestReservedFieldNames
                .Contains(field.FieldName, StringComparer.OrdinalIgnoreCase) ?
                    $"@{field.FieldName}" :
                    field.FieldName;

            key = $"{Name}{Constants.TextPartSeparator}{key}";

            agg.DateHistogram(key, t =>
            {
                t.Field(field.FieldName)
                .CalendarInterval(
                    //we use this so we can pass in string arguments - primary objective is to simplify accepting serialized queries from web clients
                    Interval switch
                    {
                        Constants.DateIntervals.Minute => DateInterval.Minute,
                        Constants.DateIntervals.Hour => DateInterval.Hour,
                        Constants.DateIntervals.Day => DateInterval.Day,
                        Constants.DateIntervals.Week => DateInterval.Week,
                        Constants.DateIntervals.Month => DateInterval.Month,
                        Constants.DateIntervals.Quarter => DateInterval.Quarter,
                        Constants.DateIntervals.Year => DateInterval.Year,
                        _ => throw new InvalidOperationException($"Provided Interval value of {Interval} does not match a known DateInterval value.  Known values are exposed in ${nameof(Constants)}.{nameof(Constants.DateIntervals)}")
                    }
                )
                .Offset(Offset)
                .MinimumDocumentCount(MinBucketSize);

                if (ExtendedBoundsMin.HasValue && ExtendedBoundsMax.HasValue)
                {
                    t.ExtendedBounds(
                        DateMath.Anchored(ExtendedBoundsMin.Value), 
                        DateMath.Anchored(ExtendedBoundsMax.Value));
                }

                return t;
            });

            return agg;
        }
        public override AggregationContainerDescriptor<T> GetAggregationDescriptor<T>(
            IAggregationField field)
        {
            var res = new AggregationContainerDescriptor<T>();

            ApplyAggregationDescriptor(res, field);

            return res;
        }

        public override IAggregationResult BuildAggregationResult(
            AggregateDictionary aggs,
            string aggKey, 
            string fieldName, 
            IAggregationCache cache)
        {
            return new DateHistogramAggregationResult(aggs, aggKey, fieldName);
        }

    }

}
