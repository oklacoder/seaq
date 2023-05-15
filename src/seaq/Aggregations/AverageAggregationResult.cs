using Nest;
using System;

namespace seaq
{
    public class AverageAggregationResult :
        IAggregationResult
    {
        public string FieldName { get; set; }
        public double? Value { get; set; }

        public AverageAggregationResult(
            AggregateDictionary aggs, 
            string aggKey,
            string fieldName)
        {
            var a = aggs.Average(aggKey);

            if (a == null)
                throw new InvalidOperationException($"Could not resolve key {aggKey} to a valid {GetType().Name}");

            FieldName = fieldName;
            Value = a.Value;
        }
    }
}
