using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace seaq
{

    public interface IAggregation
    {
        public string Name { get; }
        public AggregationContainerDescriptor<T> GetAggregationDescriptor<T>(
            IAggregationField field)
            where T : BaseDocument;
        public AggregationContainerDescriptor<T> ApplyAggregationDescriptor<T>(
            AggregationContainerDescriptor<T> agg, IAggregationField field)
            where T : BaseDocument;

        public IAggregationResult BuildAggregationResult(
            Nest.AggregateDictionary aggs,
            string aggKey,
            string fieldName);
    }
}
