using Nest;
using System.Collections.Generic;

namespace seaq
{
    public interface IAggregationCache
    {
        public Dictionary<string, IAggregation> AggregationsDictionary { get; }
        public IEnumerable<IAggregation> Aggregations { get; }

        public AggregationContainerDescriptor<T> ApplyAggregationContainer<T>(string aggregationName, AggregationContainerDescriptor<T> desc, IAggregationField field) where T : BaseDocument;
        public AggregationContainerDescriptor<T> GetAggregationContainer<T>(string aggregationName, IAggregationField field) where T : BaseDocument;
        public IAggregationResult BuildAggregationResult(string aggregationKey, AggregateDictionary aggs);
    }
}
