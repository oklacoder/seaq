using Nest;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class DefaultAggregationCache :
        IAggregationCache
    {
        public DefaultAggregationCache()
        {
            AggregationsDictionary.Add(AverageAggregation.Name, AverageAggregation);
            AggregationsDictionary.Add(DateHistogramAggregation.Name, DateHistogramAggregation);
            AggregationsDictionary.Add(HistogramAggregation.Name, HistogramAggregation);
            AggregationsDictionary.Add(MaxAggregation.Name, MaxAggregation);
            AggregationsDictionary.Add(MinAggregation.Name, MinAggregation);
            AggregationsDictionary.Add(PercentilesAggregation.Name, PercentilesAggregation);
            AggregationsDictionary.Add(StatsAggregation.Name, StatsAggregation);
            AggregationsDictionary.Add(SumAggregation.Name, SumAggregation);
            AggregationsDictionary.Add(TermsAggregation.Name, TermsAggregation);
        }

        public virtual AggregationContainerDescriptor<T> GetAggregationContainer<T>(
            string aggregationName,
            IAggregationField field)
            where T : BaseDocument
        {
            if (AggregationsDictionary.TryGetValue(aggregationName, out var aggregationContainer))
            {
                return aggregationContainer.GetAggregationDescriptor<T>(field);
            }
            else
            {
                throw new KeyNotFoundException($"No Aggregation with name {aggregationName} exists on the cluster.");
            }
        }
        public virtual AggregationContainerDescriptor<T> ApplyAggregationContainer<T>(
            string aggregationName,
            AggregationContainerDescriptor<T> desc,
            IAggregationField field)
            where T : BaseDocument
        {
            if (AggregationsDictionary.TryGetValue(aggregationName, out var aggregationContainer))
            {
                return aggregationContainer.ApplyAggregationDescriptor(desc, field);
            }
            else
            {
                throw new KeyNotFoundException($"No Aggregation with name {aggregationName} exists on the cluster.");
            }
        }
        public virtual IAggregationResult BuildAggregationResult(
            string aggregationKey,
            Nest.AggregateDictionary aggs,
            IAggregationCache cache)
        {
            var aggregationName = aggregationKey.Split(Constants.TextPartSeparator).FirstOrDefault();
            var fieldName = aggregationKey.Split(Constants.TextPartSeparator).LastOrDefault();

            if (AggregationsDictionary.TryGetValue(aggregationName, out var aggregationContainer))
            {
                return aggregationContainer.BuildAggregationResult(aggs, aggregationKey, fieldName, cache);
            }
            else
            {
                throw new KeyNotFoundException($"No Aggregation with name {aggregationName} exists on the cluster.");
            }                
        }

        public virtual Dictionary<string, IAggregation> AggregationsDictionary { get; private set; } = new Dictionary<string, IAggregation>();
        public IEnumerable<IAggregation> Aggregations => AggregationsDictionary.Values;

        public static AverageAggregation AverageAggregation { get; } = new AverageAggregation();
        public static DateHistogramAggregation DateHistogramAggregation { get; } = new DateHistogramAggregation();
        public static HistogramAggregation HistogramAggregation { get; } = new HistogramAggregation();
        public static MaxAggregation MaxAggregation { get; } = new MaxAggregation();
        public static MinAggregation MinAggregation { get; } = new MinAggregation();
        public static PercentilesAggregation PercentilesAggregation { get; } = new PercentilesAggregation();
        public static StatsAggregation StatsAggregation { get; } = new StatsAggregation();
        public static SumAggregation SumAggregation { get; } = new SumAggregation();
        public static TermsAggregation TermsAggregation { get; } = new TermsAggregation();

    }
}
