using Nest;
using System;

namespace seaq
{
    public class MaxAggregationResult :
        IAggregationResult
    {
        public string FieldName { get; set; }
        public double? Value { get; set; }

        public MaxAggregationResult()
        {

        }
        public MaxAggregationResult(
            AggregateDictionary aggs,
            string aggKey,
            string fieldName)
        {
            var a = aggs.Max(aggKey);

            if (a == null)
                throw new InvalidOperationException($"Could not resolve key {aggKey} to a valid {GetType().Name}");

            FieldName = fieldName;
            Value = a.Value;
        }
    }
}
