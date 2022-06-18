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
    public interface IAggregationCache
    {
        public Dictionary<string, IAggregation> AggregationsDictionary { get; }
        public IEnumerable<IAggregation> Aggregations { get; }

        public AggregationContainerDescriptor<T> ApplyAggregationContainer<T>(string aggregationName, AggregationContainerDescriptor<T> desc, IAggregationField field) where T : BaseDocument;
        public AggregationContainerDescriptor<T> GetAggregationContainer<T>(string aggregationName, IAggregationField field) where T : BaseDocument;
        public IAggregationResult BuildAggregationResult(string aggregationKey, AggregateDictionary aggs);
    }

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
    public interface IAggregationField
    {
        public string FieldName { get; }
    }
    public class DefaultAggregationField :
        IAggregationField
    {
        public string FieldName { get; set; }

        public DefaultAggregationField()
        {

        }
        public DefaultAggregationField(
            string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentNullException($"Parameter {nameof(fieldName)} is required.");
            FieldName = fieldName;
        }
    }
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
    public interface IAggregationResult
    {

    }

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

        public abstract IAggregationResult BuildAggregationResult(AggregateDictionary aggs, string aggKey, string fieldName);

    }

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

        public override AggregationContainerDescriptor<T> GetAggregationDescriptor<T>(
            IAggregationField field)
        {
            var res = new AggregationContainerDescriptor<T>();

            ApplyAggregationDescriptor(res, field);

            return res;
        }

        public override IAggregationResult BuildAggregationResult(AggregateDictionary aggs, string aggKey, string fieldName)
        {
            return new TermsAggregationResult(aggs, aggKey, fieldName); 
        }

    }

    public class TermsAggregationResult :
        IAggregationResult
    {
        public string FieldName { get; set; }
        public IEnumerable<DefaultBucketResult> Buckets { get; set; }

        public TermsAggregationResult()
        {

        }

        public TermsAggregationResult(
            AggregateDictionary aggs,
            string aggKey,
            string fieldName)
        {
            var a = aggs.Terms(aggKey);

            if (a == null)
                throw new InvalidOperationException($"Could not resolve key {aggKey} to a valid {GetType().Name}");

            FieldName = fieldName;
            Buckets = a.Buckets.Select(x => new DefaultBucketResult(fieldName, x.Key, x.DocCount));
        }
    }

    public class AverageAggregation :
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

            agg.Average(key, t => t
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

        public AverageAggregation()
        {

        }

        public override IAggregationResult BuildAggregationResult(AggregateDictionary aggs, string aggKey, string fieldName)
        {
            return new AverageAggregationResult(aggs, aggKey, fieldName);
        }

    }

    public class AverageAggregationResult :
        IAggregationResult
    {
        public string FieldName { get; set; }
        public double? Value { get; set; }

        public AverageAggregationResult(
            AggregateDictionary aggs, 
            string aggKey,
            string fieldName)
        {
            var a = aggs.Average(aggKey);

            if (a == null)
                throw new InvalidOperationException($"Could not resolve key {aggKey} to a valid {GetType().Name}");

            FieldName = fieldName;
            Value = a.Value;
        }
    }

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

        public override IAggregationResult BuildAggregationResult(AggregateDictionary aggs, string aggKey, string fieldName)
        {
            return new MinAggregationResult(aggs, aggKey, fieldName);
        }

    }

    public class MinAggregationResult :
        IAggregationResult
    {
        public string FieldName { get; set; }
        public double? Value { get; set; }

        public MinAggregationResult()
        {

        }
        public MinAggregationResult(
            AggregateDictionary aggs,
            string aggKey,
            string fieldName)
        {
            var a = aggs.Min(aggKey);

            if (a == null)
                throw new InvalidOperationException($"Could not resolve key {aggKey} to a valid {GetType().Name}");

            FieldName = fieldName;
            Value = a.Value;
        }
    }

    public class MaxAggregation :
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

            agg.Max(key, t => t
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

        public override IAggregationResult BuildAggregationResult(AggregateDictionary aggs, string aggKey, string fieldName)
        {
            return new MaxAggregationResult(aggs, aggKey, fieldName);
        }

    }
    public class MaxAggregationResult :
        IAggregationResult
    {
        public string FieldName { get; set; }
        public double? Value { get; set; }

        public MaxAggregationResult()
        {

        }
        public MaxAggregationResult(
            AggregateDictionary aggs,
            string aggKey,
            string fieldName)
        {
            var a = aggs.Max(aggKey);

            if (a == null)
                throw new InvalidOperationException($"Could not resolve key {aggKey} to a valid {GetType().Name}");

            FieldName = fieldName;
            Value = a.Value;
        }
    }

    public class SumAggregation :
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

            agg.Sum(key, t => t
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

        public override IAggregationResult BuildAggregationResult(AggregateDictionary aggs, string aggKey, string fieldName)
        {
            return new SumAggregationResult(aggs, aggKey, fieldName);
        }

    }
    public class SumAggregationResult :
        IAggregationResult
    {
        public string FieldName { get; set; }
        public double? Value { get; set; }

        public SumAggregationResult()
        {

        }
        public SumAggregationResult(
            AggregateDictionary aggs,
            string aggKey,
            string fieldName)
        {
            var a = aggs.Sum(aggKey);

            if (a == null)
                throw new InvalidOperationException($"Could not resolve key {aggKey} to a valid {GetType().Name}");

            FieldName = fieldName;
            Value = a.Value;
        }
    }

    public class PercentilesAggregation :
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

            agg.Percentiles(key, t => t
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

        public override IAggregationResult BuildAggregationResult(AggregateDictionary aggs, string aggKey, string fieldName)
        {
            return new PercentilesAggregationResult(aggs, aggKey, fieldName);
        }

    }
    public class PercentilesAggregationResult :
        IAggregationResult
    {
        public string FieldName { get; set; }
        public IEnumerable<DefaultPercentileResult> Percentiles { get; set; }        

        public PercentilesAggregationResult()
        {

        }
        public PercentilesAggregationResult(
            AggregateDictionary aggs,
            string aggKey,
            string fieldName)
        {
            var a = aggs.Percentiles(aggKey);

            if (a == null)
                throw new InvalidOperationException($"Could not resolve key {aggKey} to a valid {GetType().Name}");

            FieldName = fieldName;
            Percentiles = a?.Items?.Select(x => new DefaultPercentileResult(fieldName, x.Percentile, x.Value)) ?? Array.Empty<DefaultPercentileResult>();
        }
    }

    public class StatsAggregation :
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

            agg.Stats(key, t => t
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

        public override IAggregationResult BuildAggregationResult(AggregateDictionary aggs, string aggKey, string fieldName)
        {
            return new StatsAggregationResult(aggs, aggKey, fieldName);
        }

    }
    public class StatsAggregationResult :
        IAggregationResult
    {
        public string FieldName { get; set; }
        public double? Average { get; set; }
        public double? Count { get; set; }
        public double? Max { get; set; }
        public double? Min { get; set; }
        public double? Sum { get; set; }

        public StatsAggregationResult()
        {

        }
        public StatsAggregationResult(
            AggregateDictionary aggs,
            string aggKey,
            string fieldName)
        {
            var a = aggs.Stats(aggKey);

            if (a == null)
                throw new InvalidOperationException($"Could not resolve key {aggKey} to a valid {GetType().Name}");

            FieldName = fieldName;
            Average = a?.Average;
            Count = a?.Count;
            Max = a?.Max;
            Min = a?.Min;
            Sum = a?.Sum;
        }
    }




    public class AggregationQuery<T> :
        ISeaqQuery<T>
        where T : BaseDocument
    {
        private AggregationQueryCriteria<T> _criteria;
        public ISeaqQueryCriteria<T> Criteria => _criteria;

        public AggregationQuery(
            AggregationQueryCriteria<T> criteria)
        {
            _criteria = criteria;
        }

        public ISeaqQueryResults<T> Execute(
            ElasticClient client)
        {

            var query = _criteria.GetSearchDescriptor();

            var results = client.Search<T>(query);

            return new AggregationQueryResults<T>(results, _criteria);
        }

        public Task<ISeaqQueryResults<T>> ExecuteAsync(
            ElasticClient client)
        {
            throw new NotImplementedException();
        }
    }
    public class AggregationQueryCriteria<T> :
        ISeaqQueryCriteria<T>
        where T : BaseDocument
    {
        /// <summary>
        /// Query text
        /// </summary>
        [DataMember(Name = "text")]
        [JsonPropertyName("text")]
        public string Text { get; private set; }

        /// <summary>
        /// Specify which indices to query.  If empty or null, query will default to the default index for the provided type.
        /// </summary>
        [DataMember(Name = "indices")]
        [JsonPropertyName("indices")]
        public string[] Indices { get; private set; }

        /// <summary>
        /// Used for paging.  Note that this is only deterministic if consistent sort fields are provided on each related query.
        /// </summary>
        [DataMember(Name = "skip")]
        [JsonPropertyName("skip")]
        public int? Skip { get; private set; } = 0;

        /// <summary>
        /// Used for paging.  Note that this is only deterministic if consistent sort fields are provided on each related query.
        /// </summary>
        [DataMember(Name = "take")]
        [JsonPropertyName("take")]
        public int? Take { get; private set; } = 0;

        [DataMember(Name = "aggregations")]
        [JsonPropertyName("aggregations")]
        public IEnumerable<DefaultAggregationRequest> AggregationRequests { get; private set; }

        /// <summary>
        /// Collection of FilterField objects used to construct the query
        /// </summary>
        [DataMember(Name = "filterFields")]
        [JsonPropertyName("filterFields")]
        private IEnumerable<DefaultFilterField> _filterFields { get; set; }
        public IEnumerable<IFilterField> FilterFields => _filterFields;

        /// <summary>
        /// Collection of SortField objects used to order the query results
        /// </summary>
        [DataMember(Name = "sortFields")]
        [JsonPropertyName("sortFields")]
        private IEnumerable<DefaultSortField> _sortFields { get; set; }
        public IEnumerable<ISortField> SortFields => _sortFields;

        /// <summary>
        /// Collection of ReturnField objects used to limit tthe fields included in the query results
        /// </summary>
        [DataMember(Name = "returnFields")]
        [JsonPropertyName("returnFields")]
        private IEnumerable<DefaultReturnField> _returnFields { get; set; }
        public IEnumerable<IReturnField> ReturnFields => _returnFields;

        /// <summary>
        /// Collection of BucketField objects used to control returned terms aggregations for further filtering
        /// </summary>
        [DataMember(Name = "bucketFields")]
        [JsonPropertyName("bucketFields")]
        public IEnumerable<DefaultBucketField> _bucketFields { get; set; }
        public IEnumerable<IBucketField> BucketFields => _bucketFields;

        /// <summary>
        /// Collection of strings used to control which fields are used to calculate score boosting
        /// </summary>
        [DataMember(Name = "boostedFields")]
        [JsonPropertyName("boostedFields")]
        public IEnumerable<string> BoostedFields { get; private set; } = new string[] { "*" };

        /// <summary>
        /// Indexes targeted by this query that are marked as "deprecated" on the containing cluster
        /// </summary>
        [DataMember(Name = "deprecatedIndexTargets")]
        [JsonPropertyName("deprecatedIndexTargets")]
        public IEnumerable<string> DeprecatedIndexTargets { get; private set; } = Enumerable.Empty<string>();
        /// <summary>
        /// Override cluster settings for boosted/return fields, giving full preference to the values provided in the provided Criteria object
        /// </summary>
        [DataMember(Name = "overrideClusterSettings")]
        [JsonPropertyName("overrideClusterSettings")]
        public bool OverrideClusterSettings { get; }

        internal IAggregationCache _aggregationCache;

        public AggregationQueryCriteria(
            string text = null,
            IEnumerable<string> indices = null,
            IEnumerable<DefaultFilterField> filterFields = null,
            IEnumerable<DefaultAggregationRequest> aggregationRequests = null)
        {
            Text = text;
            Indices = indices?.ToArray() ?? Array.Empty<string>();
            _filterFields = filterFields;
            AggregationRequests = aggregationRequests;
        }

        public SearchDescriptor<T> GetSearchDescriptor()
        {
            var s = new SearchDescriptor<T>()
                .Index(Indices)
                .Take(0)
                .Aggregations(a => AggregationRequests.GetAggregationsContainer<T>(_aggregationCache))
                .Query(x => x.GetQueryContainerDescriptor(Text, FilterFields, boostFields: BoostedFields))
                .Source(t => t.ExcludeAll());

            return s;
        }

        public void ApplyClusterSettings(Cluster cluster)
        {
            ApplyClusterIndices(cluster);
            _aggregationCache = cluster.AggregationCache;
        }

        internal void ApplyClusterIndices(Cluster cluster)
        {
            if (Indices?.Any() is true)
            {
                if (Indices.Any(z => cluster.DeprecatedIndices.Any(x => x.Name.Equals(z, StringComparison.OrdinalIgnoreCase))))
                {
                    var deps = cluster.DeprecatedIndices.Where(x => Indices.Any(z => z.Equals(x.Name, StringComparison.OrdinalIgnoreCase)));

                    DeprecatedIndexTargets = deps.Select(x => $"{x.Name} is deprecated - {x.DeprecationMessage}");
                }
                return;
            }
            IEnumerable<Index> idx;
            if (string.IsNullOrWhiteSpace(typeof(T).FullName))
            {
                idx = cluster.Indices.Where(x =>
                    x.IsHidden is not true &&
                    x.ReturnInGlobalSearch is true);
            }
            else
            {
                idx = cluster.IndicesByType[typeof(T).FullName];
                DeprecatedIndexTargets = idx.Where(x => x.IsDeprecated).Select(x => $"{x.Name} is deprecated - {x.DeprecationMessage}");
                idx = idx.Where(x => x.IsHidden is not true);
            }

            Indices = idx.Select(x => x.Name).ToArray();
            if (Indices?.Any() is not true)
            {
                throw new InvalidOperationException($"No indices could be identified for type {typeof(T).FullName}.  Query could not be processed.  " +
                    $"Ensure that either an index exists on your seaq cluster fo this type, or that you have specified an explicit type in your query definition.");
            }
        }
        
    }
    public class AggregationQueryResults<T> :
        ISeaqQueryResults<T>
        where T : BaseDocument
    {
        /// <summary>
        /// Collection of query result objects
        /// </summary>
        public IEnumerable<DefaultQueryResult<T>> Results { get; }
        /// <summary>
        /// Collection of query result objects
        /// </summary>
        IEnumerable<ISeaqQueryResult<T>> ISeaqQueryResults<T>.Results => Results;

        public IEnumerable<IAggregationResult> AggregationResults { get; }

        public long Took { get; }

        public long Total { get; }

        /// <summary>
        /// Additional information about this query execution
        /// </summary>
        public IEnumerable<string> Messages { get; set; } = Enumerable.Empty<string>();

        public AggregationQueryResults(
            Nest.ISearchResponse<T> searchResponse,
            AggregationQueryCriteria<T> criteria,
            IEnumerable<string> messages = null)
        {
            if (searchResponse.IsValid)
            {
                Results = Array.Empty<DefaultQueryResult<T>>();

                AggregationResults = QueryHelper.BuildAggregationsResult(searchResponse.Aggregations, criteria);

                Total = searchResponse.Total;
                Took = searchResponse.Took;

                Messages = messages;
            }
            else
            {
                Results = null;
                AggregationResults = null;
                Total = searchResponse.Total;
                Took = searchResponse.Took;

                Messages = new[] { searchResponse?.OriginalException.Message };
            }

        }
    }
}
