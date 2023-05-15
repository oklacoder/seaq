using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
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

}
