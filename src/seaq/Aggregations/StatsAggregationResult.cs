using Nest;
using System;

namespace seaq
{
    public class StatsAggregationResult :
        IAggregationResult
    {
        public string FieldName { get; set; }
        public double? Average { get; set; }
        public double? Count { get; set; }
        public double? Max { get; set; }
        public double? Min { get; set; }
        public double? Sum { get; set; }

        public StatsAggregationResult()
        {

        }
        public StatsAggregationResult(
            AggregateDictionary aggs,
            string aggKey,
            string fieldName)
        {
            var a = aggs.Stats(aggKey);

            if (a == null)
                throw new InvalidOperationException($"Could not resolve key {aggKey} to a valid {GetType().Name}");

            FieldName = fieldName;
            Average = a?.Average;
            Count = a?.Count;
            Max = a?.Max;
            Min = a?.Min;
            Sum = a?.Sum;
        }
    }
}
