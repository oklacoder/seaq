using Nest;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class TermsAggregationRequest :
        DefaultAggregationRequest
    {
        private int _size { get; set; } = 10;
        private IEnumerable<DefaultAggregationRequest> _aggregations;

        public TermsAggregationRequest(
            DefaultAggregationField field,
            IEnumerable<DefaultAggregationRequest> aggregations = null,
            int? size = null)
            : base (DefaultAggregationCache.TermsAggregation.Name, field)
        {
            _aggregations = aggregations;
            if (size.HasValue)
                _size = size.Value;
        }

        public override AggregationContainerDescriptor<T> ApplyAggregationDescriptor<T>(
            AggregationContainerDescriptor<T> agg,
            IAggregationCache aggregationCache)
        {
            //return _aggregations?.Any() is true ?
            //    new TermsAggregation().ApplyAggregationDescriptor<T>(agg, Field, _aggregations, aggregationCache, size: _size) :
            //    new TermsAggregation().ApplyAggregationDescriptor<T>(agg, Field);
            return new TermsAggregation().ApplyAggregationDescriptor<T>(agg, Field, _aggregations, aggregationCache, size: _size);
        }
        public override AggregationContainerDescriptor<T> GetAggregationDescriptor<T>(
            IAggregationCache aggregationCache)
        {
            return _aggregations?.Any() is true ?
                new TermsAggregation().GetAggregationDescriptor<T>(Field, _aggregations, aggregationCache, size: _size) :
                new TermsAggregation().GetAggregationDescriptor<T>(Field);
        }
    }
}
