using Nest;

namespace seaq
{
    public abstract class BaseAggregation :
        IAggregation
    {
        public string Name => GetType().FullName;

        public abstract AggregationContainerDescriptor<T> ApplyAggregationDescriptor<T>(
            AggregationContainerDescriptor<T> agg,
            IAggregationField field)
            where T : BaseDocument;

        public abstract AggregationContainerDescriptor<T> GetAggregationDescriptor<T>(
            IAggregationField field)
            where T : BaseDocument;

        public abstract IAggregationResult BuildAggregationResult(AggregateDictionary aggs, string aggKey, string fieldName, IAggregationCache cache);

    }
}
