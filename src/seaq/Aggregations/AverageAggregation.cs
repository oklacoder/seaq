using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class AverageAggregation :
        BaseAggregation
    {
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

            agg.Average(key, t => t
                .Field(field.FieldName)
                .Missing(0));

            return agg;
        }

        public override AggregationContainerDescriptor<T> GetAggregationDescriptor<T>(
            IAggregationField field)
        {
            var res = new AggregationContainerDescriptor<T>();

            ApplyAggregationDescriptor(res, field);

            return res;
        }

        public AverageAggregation()
        {

        }

        public override IAggregationResult BuildAggregationResult(AggregateDictionary aggs, string aggKey, string fieldName)
        {
            return new AverageAggregationResult(aggs, aggKey, fieldName);
        }

    }

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

    public class DateHistogramAggregation :
        BaseAggregation
    {
        public string Interval { get; set; } = Constants.DateIntervals.Day;
        public string Offset { get; set; } = "0";
        public int MinBucketSize { get; set; } = 1;

        public DateHistogramAggregation()
        {

        }
        public DateHistogramAggregation(
            string? interval = null, 
            string? offset = null, 
            int? minBucketSize = null)
        {
            if (!string.IsNullOrWhiteSpace(interval))
                Interval = interval;
            if (!string.IsNullOrWhiteSpace(offset))
                Offset = offset;
            if (minBucketSize.HasValue)
                MinBucketSize = minBucketSize.Value;
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

            agg.DateHistogram(key, t => t
                .Field(field.FieldName)
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
                .MinimumDocumentCount(MinBucketSize));

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
            string fieldName)
        {
            return new DateHistogramAggregationResult(aggs, aggKey, fieldName);
        }

    }


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


    public class DateHistogramAggregationResult :
        IAggregationResult
    {
        public string FieldName { get; }
        public IEnumerable<DefaultBucketResult> Buckets { get; }

        public DateHistogramAggregationResult(
            AggregateDictionary aggs,
            string aggKey,
            string fieldName)
        {
            var a = aggs.DateHistogram(aggKey);

            if (a == null)
                throw new InvalidOperationException($"Could not resolve key {aggKey} to a valid {GetType().Name}");

            FieldName = fieldName;

            Buckets = a.Buckets.Select(b =>
                new DefaultBucketResult(b.KeyAsString, b.KeyAsString, b.DocCount));
        }
    }
    public class HistogramAggregationResult :
        IAggregationResult
    {
        public string FieldName { get; }
        public IEnumerable<DefaultBucketResult> Buckets { get; }

        public HistogramAggregationResult(
            AggregateDictionary aggs,
            string aggKey,
            string fieldName)
        {
            var a = aggs.Histogram(aggKey);

            if (a == null)
                throw new InvalidOperationException($"Could not resolve key {aggKey} to a valid {GetType().Name}");

            FieldName = fieldName;

            Buckets = a.Buckets.Select(b =>
                new DefaultBucketResult(b.KeyAsString, b.KeyAsString, b.DocCount));
        }
    }

}
