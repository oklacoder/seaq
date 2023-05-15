using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class TermsAggregationResult :
        IAggregationResult
    {
        public string FieldName { get; set; }
        public IEnumerable<DefaultBucketResult> Buckets { get; set; }

        public TermsAggregationResult()
        {

        }

        public TermsAggregationResult(
            AggregateDictionary aggs,
            string aggKey,
            string fieldName,
            IAggregationCache cache)
        {
            var a = aggs.Terms(aggKey);

            if (a == null)
                throw new InvalidOperationException($"Could not resolve key {aggKey} to a valid {GetType().Name}");

            FieldName = fieldName;
            Buckets = a?.Buckets?.Select(x => 
                x.Any() is true ? 
                    new NestedBucketResult(fieldName, x.Key, x.DocCount, x, cache) :
                    new DefaultBucketResult(fieldName, x.Key, x.DocCount));
        }
    }
}
