using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
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
