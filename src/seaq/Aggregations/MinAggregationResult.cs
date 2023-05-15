using Nest;
using System;

namespace seaq
{
    public class MinAggregationResult :
        IAggregationResult
    {
        public string FieldName { get; set; }
        public double? Value { get; set; }

        public MinAggregationResult()
        {

        }
        public MinAggregationResult(
            AggregateDictionary aggs,
            string aggKey,
            string fieldName)
        {
            var a = aggs.Min(aggKey);

            if (a == null)
                throw new InvalidOperationException($"Could not resolve key {aggKey} to a valid {GetType().Name}");

            FieldName = fieldName;
            Value = a.Value;
        }
    }
}
