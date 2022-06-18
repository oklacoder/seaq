using Nest;
using System;

namespace seaq
{
    public class DefaultAggregationRequest :
        IAggregationRequest
    {
        public string AggregationName { get; set; }

        private DefaultAggregationField _field { get; set; }
        public IAggregationField Field => _field;

        public AggregationContainerDescriptor<T> ApplyAggregationDescriptor<T>(
            AggregationContainerDescriptor<T> agg,
            IAggregationCache aggregationCache) 
            where T : BaseDocument
        {

            return aggregationCache.ApplyAggregationContainer<T>(AggregationName, agg, Field);
        }
        public AggregationContainerDescriptor<T> GetAggregationDescriptor<T>(
            IAggregationCache aggregationCache)
            where T : BaseDocument
        {
            return aggregationCache.GetAggregationContainer<T>(AggregationName, Field);
        }

        public DefaultAggregationRequest()
        {

        }

        public DefaultAggregationRequest(
            string aggregationName,
            DefaultAggregationField field)
        {
            if (string.IsNullOrWhiteSpace(aggregationName))
                throw new ArgumentNullException(nameof(aggregationName));

            _field = field;
            AggregationName = aggregationName;
        }
    }
}
