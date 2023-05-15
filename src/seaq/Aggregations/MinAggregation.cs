using Nest;
using System;
using System.Linq;

namespace seaq
{
    public class MinAggregation :
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

            agg.Min(key, t => t
                .Field(field.FieldName)
                .Missing(0));

            return agg;
        }

        public override AggregationContainerDescriptor<T> GetAggregationDescriptor<T>(
            IAggregationField field)
        {
            var res = new AggregationContainerDescriptor<T>();

            ApplyAggregationDescriptor(res, field);

            return res;
        }

        public override IAggregationResult BuildAggregationResult(
            AggregateDictionary aggs,
            string aggKey,
            string fieldName,
            IAggregationCache cache)
        {
            return new MinAggregationResult(aggs, aggKey, fieldName);
        }

    }
}
