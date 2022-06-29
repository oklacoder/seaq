using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class TermsAggregation :
        BaseAggregation
    {
        public override AggregationContainerDescriptor<T> ApplyAggregationDescriptor<T>(
            AggregationContainerDescriptor<T> agg,
            IAggregationField field)
        {
            if (string.IsNullOrWhiteSpace(field?.FieldName))
                return agg;

            var key = Constants.Fields.NestReservedFieldNames
                .Contains(field.FieldName, StringComparer.OrdinalIgnoreCase) ?
                    $"@{field.FieldName}" :
                    field.FieldName;

            key = $"{Name}{Constants.TextPartSeparator}{key}";

            agg.Terms(key, t => t
                .Field(field.FieldName)
                .MinimumDocumentCount(2)
                .Order(o => o.CountDescending().KeyAscending()));

            return agg;
        }
        public AggregationContainerDescriptor<T> ApplyAggregationDescriptor<T>(
            AggregationContainerDescriptor<T> agg,
            IAggregationField field,
            IEnumerable<DefaultAggregationRequest> nestedAggregations,
            IAggregationCache aggregationCache)
            where T : BaseDocument
        {
            if (string.IsNullOrWhiteSpace(field?.FieldName))
                return agg;

            var key = Constants.Fields.NestReservedFieldNames
                .Contains(field.FieldName, StringComparer.OrdinalIgnoreCase) ?
                    $"@{field.FieldName}" :
                    field.FieldName;

            key = $"{Name}{Constants.TextPartSeparator}{key}";

            agg.Terms(key, t => t
                .Field(field.FieldName)
                .MinimumDocumentCount(2)
                .Aggregations(x => 
                {
                    if (nestedAggregations?.Any() is true)
                        foreach(var a in nestedAggregations)
                            a.ApplyAggregationDescriptor(x, aggregationCache);

                    return x;
                })
                .Order(o => o.CountDescending().KeyAscending()));

            return agg;
        }

        public override AggregationContainerDescriptor<T> GetAggregationDescriptor<T>(
            IAggregationField field)
        {
            var res = new AggregationContainerDescriptor<T>();

            ApplyAggregationDescriptor(res, field);

            return res;
        }
        public AggregationContainerDescriptor<T> GetAggregationDescriptor<T>(
            IAggregationField field,
            IEnumerable<DefaultAggregationRequest> nestedAggregations,
            IAggregationCache aggregationCache)
            where T : BaseDocument
        {
            var res = new AggregationContainerDescriptor<T>();

            ApplyAggregationDescriptor(res, field, nestedAggregations, aggregationCache);

            return res;
        }

        public override IAggregationResult BuildAggregationResult(
            AggregateDictionary aggs,
            string aggKey,
            string fieldName,
            IAggregationCache cache)
        {
            return new TermsAggregationResult(aggs, aggKey, fieldName, cache); 
        }

    }
}
