using Nest;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class DefaultBucketResult :
        IBucketResult
    {
        public string Key { get; }
        public string Value { get; }
        public long? Count { get; }

        public DefaultBucketResult()
        {

        }
        public DefaultBucketResult(
            string key,
            string value,
            long? count)
        {
            Key = key;
            Value = value;
            Count = count;
        }
    }
    public class NestedBucketResult :
        DefaultBucketResult
    {
        public IEnumerable<IAggregationResult> Nested { get; set; }

        public NestedBucketResult()
        {

        }
        public NestedBucketResult(
            string key,
            string value,
            long? count,
            AggregateDictionary aggs, 
            IAggregationCache cache)
            : base(key, value, count)
        {            
            Nested = aggs?.Keys?.Select(x => cache?.BuildAggregationResult(x, aggs, cache));
        }
    }
}
