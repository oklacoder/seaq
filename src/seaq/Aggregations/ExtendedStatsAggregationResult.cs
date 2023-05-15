using Nest;
using System;

namespace seaq
{
    public class ExtendedStatsAggregationResult :
        IAggregationResult
    {
        public string FieldName { get; set; }
        public double? Average { get; set; }
        public double? Count { get; set; }
        public double? Max { get; set; }
        public double? Min { get; set; }
        public double? StandardDeviation { get; set; }
        public double? StandardDeviationPopulation { get; set; }
        public double? StandardDeviationSampling { get; set; }
        public double? Sum { get; set; }
        public double? SumOfSquares { get; set; }
        public double? Variance { get; set; }
        public double? VariancePopulation { get; set; }
        public double? VarianceSampling { get; set; }
        public StandardDeviationBounds StandardDeviationBounds { get; set; }

        public ExtendedStatsAggregationResult()
        {

        }
        public ExtendedStatsAggregationResult(
            AggregateDictionary aggs,
            string aggKey,
            string fieldName)
        {
            var a = aggs.ExtendedStats(aggKey);

            if (a == null)
                throw new InvalidOperationException($"Could not resolve key {aggKey} to a valid {GetType().Name}");

            FieldName = fieldName;
            Average = a?.Average;
            Count = a?.Count;
            Max = a?.Max;
            Min = a?.Min;
            StandardDeviation = a?.StdDeviation;
            StandardDeviationBounds = a?.StdDeviationBounds;
            StandardDeviationPopulation = a?.StdDeviationPopulation;
            StandardDeviationSampling = a?.StdDeviationSampling;
            Sum = a?.Sum;
            SumOfSquares = a?.SumOfSquares;
            Variance = a?.Variance;
            VariancePopulation = a?.VariancePopulation;
            VarianceSampling = a?.VarianceSampling;
        }
    }
}
