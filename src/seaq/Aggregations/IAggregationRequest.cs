using Nest;

namespace seaq
{
    public interface IAggregationRequest
    {
        public string AggregationName { get; }
        public IAggregationField Field { get; }

        public AggregationContainerDescriptor<T> GetAggregationDescriptor<T>(
            IAggregationCache aggregationCache)
            where T : BaseDocument;

        public AggregationContainerDescriptor<T> ApplyAggregationDescriptor<T>(
            AggregationContainerDescriptor<T> agg,
            IAggregationCache aggregationCache)
            where T : BaseDocument;
    }
}
