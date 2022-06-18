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
            Nest.AggregateDictionary aggs)
        {
            var aggregationName = aggregationKey.Split(Constants.TextPartSeparator).FirstOrDefault();
            var fieldName = aggregationKey.Split(Constants.TextPartSeparator).LastOrDefault();

            if (AggregationsDictionary.TryGetValue(aggregationName, out var aggregationContainer))
            {
                return aggregationContainer.BuildAggregationResult(aggs, aggregationKey, fieldName);
            }
            else
            {
                throw new KeyNotFoundException($"No Aggregation with name {aggregationName} exists on the cluster.");
            }                
        }

        public virtual Dictionary<string, IAggregation> AggregationsDictionary { get; private set; } = new Dictionary<string, IAggregation>();
        public IEnumerable<IAggregation> Aggregations => AggregationsDictionary.Values;

        public AverageAggregation AverageAggregation => new AverageAggregation();
        public MaxAggregation MaxAggregation => new MaxAggregation();
        public MinAggregation MinAggregation => new MinAggregation();
        public PercentilesAggregation PercentilesAggregation => new PercentilesAggregation();
        public StatsAggregation StatsAggregation => new StatsAggregation();
        public SumAggregation SumAggregation => new SumAggregation();
        public TermsAggregation TermsAggregation => new TermsAggregation();
    }
}
