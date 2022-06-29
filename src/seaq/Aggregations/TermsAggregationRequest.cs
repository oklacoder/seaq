using Nest;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class TermsAggregationRequest :
        DefaultAggregationRequest
    {
        private IEnumerable<DefaultAggregationRequest> _aggregations;

        public TermsAggregationRequest(
            DefaultAggregationField field,
            IEnumerable<DefaultAggregationRequest> aggregations = null)
            : base (DefaultAggregationCache.TermsAggregation.Name, field)
        {
            _aggregations = aggregations;
        }

        public override AggregationContainerDescriptor<T> ApplyAggregationDescriptor<T>(
            AggregationContainerDescriptor<T> agg,
            IAggregationCache aggregationCache)
        {
            return _aggregations?.Any() is true ?
                new TermsAggregation().ApplyAggregationDescriptor<T>(agg, Field, _aggregations, aggregationCache) :
                new TermsAggregation().ApplyAggregationDescriptor<T>(agg, Field);
        }
        public override AggregationContainerDescriptor<T> GetAggregationDescriptor<T>(
            IAggregationCache aggregationCache)
        {
            return _aggregations?.Any() is true ?
                new TermsAggregation().GetAggregationDescriptor<T>(Field, _aggregations, aggregationCache) :
                new TermsAggregation().GetAggregationDescriptor<T>(Field);
        }
    }
}
