using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class PercentilesAggregationResult :
        IAggregationResult
    {
        public string FieldName { get; set; }
        public IEnumerable<DefaultPercentileResult> Percentiles { get; set; }        

        public PercentilesAggregationResult()
        {

        }
        public PercentilesAggregationResult(
            AggregateDictionary aggs,
            string aggKey,
            string fieldName)
        {
            var a = aggs.Percentiles(aggKey);

            if (a == null)
                throw new InvalidOperationException($"Could not resolve key {aggKey} to a valid {GetType().Name}");

            FieldName = fieldName;
            Percentiles = a?.Items?.Select(x => new DefaultPercentileResult(fieldName, x.Percentile, x.Value)) ?? Array.Empty<DefaultPercentileResult>();
        }
    }
}
