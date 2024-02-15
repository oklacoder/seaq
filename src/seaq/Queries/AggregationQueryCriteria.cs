using Nest;
using seaq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace seaq
{
    public class AggregationQueryCriteria :
        ISeaqQueryCriteria
    {
        /// <summary>
        /// Full dotnet type name of desired return objects
        /// </summary>
        [DataMember(Name = "type")]
        [JsonPropertyName("type")]
        public string Type { get; set; }
        /// <summary>
        /// Query text
        /// </summary>
        [DataMember(Name = "text")]
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Specify which indices to query.  If empty or null, query will default to the default index for the provided type.
        /// </summary>
        [DataMember(Name = "indices")]
        [JsonPropertyName("indices")]
        public IEnumerable<string> Indices { get; set; }

        /// <summary>
        /// Used for paging.  Note that this is only deterministic if consistent sort fields are provided on each related query.
        /// </summary>
        [DataMember(Name = "skip")]
        [JsonPropertyName("skip")]
        public int? Skip { get; set; } = 0;

        /// <summary>
        /// Used for paging.  Note that this is only deterministic if consistent sort fields are provided on each related query.
        /// </summary>
        [DataMember(Name = "take")]
        [JsonPropertyName("take")]
        public int? Take { get; set; } = 0;

        [DataMember(Name = "aggregations")]
        [JsonPropertyName("aggregations")]
        public IEnumerable<DefaultAggregationRequest> AggregationRequests { get; set; }

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
        public IEnumerable<string> BoostedFields { get; set; } = new string[] { "*" };

        /// <summary>
        /// Indexes targeted by this query that are marked as "deprecated" on the containing cluster
        /// </summary>
        [DataMember(Name = "deprecatedIndexTargets")]
        [JsonPropertyName("deprecatedIndexTargets")]
        public IEnumerable<string> DeprecatedIndexTargets { get; set; } = Array.Empty<string>();
        /// <summary>
        /// Override cluster settings for boosted/return fields, giving full preference to the values provided in the provided Criteria object
        /// </summary>
        [DataMember(Name = "overrideClusterSettings")]
        [JsonPropertyName("overrideClusterSettings")]
        public bool OverrideClusterSettings { get; }

        internal IAggregationCache _aggregationCache;

        public AggregationQueryCriteria(
            string text = null,
            string type = null,
            IEnumerable<string> indices = null,
            IEnumerable<DefaultFilterField> filterFields = null,
            IEnumerable<DefaultAggregationRequest> aggregationRequests = null)
        {
            Text = text;
            Type = type;
            Indices = indices ?? Array.Empty<string>();
            _filterFields = filterFields;
            AggregationRequests = aggregationRequests;
        }

        public SearchDescriptor<BaseDocument> GetSearchDescriptor()
        {
            var s = new SearchDescriptor<BaseDocument>()
                .Index(Indices?.ToArray() ?? Array.Empty<string>())
                .Take(0)
                .Aggregations(a => AggregationRequests.GetAggregationsContainer<BaseDocument>(_aggregationCache))
                .Query(x => x.GetQueryContainerDescriptor(Text, FilterFields, boostFields: BoostedFields))
                .Source(t => t.ExcludeAll());

            return s;
        }

        public void ApplyClusterSettings(Cluster cluster)
        {
            this.ApplyClusterIndices(cluster);
            _aggregationCache = cluster.AggregationCache;
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
        public string Text { get; set; }

        /// <summary>
        /// Specify which indices to query.  If empty or null, query will default to the default index for the provided type.
        /// </summary>
        [DataMember(Name = "indices")]
        [JsonPropertyName("indices")]
        public IEnumerable<string> Indices { get; set; }

        /// <summary>
        /// Used for paging.  Note that this is only deterministic if consistent sort fields are provided on each related query.
        /// </summary>
        [DataMember(Name = "skip")]
        [JsonPropertyName("skip")]
        public int? Skip { get; set; } = 0;

        /// <summary>
        /// Used for paging.  Note that this is only deterministic if consistent sort fields are provided on each related query.
        /// </summary>
        [DataMember(Name = "take")]
        [JsonPropertyName("take")]
        public int? Take { get; set; } = 0;

        [DataMember(Name = "aggregations")]
        [JsonPropertyName("aggregations")]
        public IEnumerable<DefaultAggregationRequest> AggregationRequests { get; set; }

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
        public IEnumerable<string> BoostedFields { get; set; } = new string[] { "*" };

        /// <summary>
        /// Indexes targeted by this query that are marked as "deprecated" on the containing cluster
        /// </summary>
        [DataMember(Name = "deprecatedIndexTargets")]
        [JsonPropertyName("deprecatedIndexTargets")]
        public IEnumerable<string> DeprecatedIndexTargets { get; set; } = Array.Empty<string>();
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
            Indices = indices ?? Array.Empty<string>();
            _filterFields = filterFields;
            AggregationRequests = aggregationRequests;
        }

        public SearchDescriptor<T> GetSearchDescriptor()
        {
            var s = new SearchDescriptor<T>()
                .Index(Indices?.ToArray() ?? Array.Empty<string>())
                .Take(0)
                .Aggregations(a => AggregationRequests.GetAggregationsContainer<T>(_aggregationCache))
                .Query(x => x.GetQueryContainerDescriptor(Text, FilterFields, boostFields: BoostedFields))
                .Source(t => t.ExcludeAll());

            return s;
        }

        public void ApplyClusterSettings(Cluster cluster)
        {
            this.ApplyClusterIndices(cluster);
            _aggregationCache = cluster.AggregationCache;
        }        
    }
}
