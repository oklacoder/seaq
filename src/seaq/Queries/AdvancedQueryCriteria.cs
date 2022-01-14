﻿using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace seaq
{
    public class AdvancedQueryCriteria :
        ISeaqQueryCriteria
    {
        /// <summary>
        /// Full dotnet type name of desired return objects
        /// </summary>
        [DataMember(Name = "type")]
        [JsonPropertyName("type")]
        public string Type { get; init; }

        /// <summary>
        /// Specify which indices to query.  If empty or null, query will default to the default index for the provided type.
        /// </summary>
        [DataMember(Name = "indices")]
        [JsonPropertyName("indices")]
        public string[] Indices { get; protected set; }

        /// <summary>
        /// Used for paging.  Note that this is only deterministic if consistent sort fields are provided on each related query.
        /// </summary>
        [DataMember(Name = "skip")]
        [JsonPropertyName("skip")]
        public int? Skip { get; init; }

        /// <summary>
        /// Used for paging.  Note that this is only deterministic if consistent sort fields are provided on each related query.
        /// </summary>
        [DataMember(Name = "take")]
        [JsonPropertyName("take")]
        public int? Take { get; init; }

        /// <summary>
        /// Collection of FilterField objects used to construct the query
        /// </summary>
        [DataMember(Name = "filterFields")]
        [JsonPropertyName("filterFields")]
        private IEnumerable<DefaultFilterField> _filterFields { get; init; }
        public IEnumerable<IFilterField> FilterFields => _filterFields;

        /// <summary>
        /// Collection of SortField objects used to order the query results
        /// </summary>
        [DataMember(Name = "sortFields")]
        [JsonPropertyName("sortFields")]
        private IEnumerable<DefaultSortField> _sortFields { get; init; }
        public IEnumerable<ISortField> SortFields => _sortFields;

        /// <summary>
        /// Collection of ReturnField objects used to limit tthe fields included in the query results
        /// </summary>
        [DataMember(Name = "returnFields")]
        [JsonPropertyName("returnFields")]
        public IEnumerable<DefaultReturnField> _returnFields { get; init; }
        public IEnumerable<IReturnField> ReturnFields => _returnFields;

        /// <summary>
        /// Collection of BucketField objects used to control returned terms aggregations for further filtering
        /// </summary>
        [DataMember(Name = "bucketFields")]
        [JsonPropertyName("bucketFields")]
        public IEnumerable<DefaultBucketField> _bucketFields { get; init; }
        public IEnumerable<IBucketField> BucketFields => _bucketFields;

        /// <summary>
        /// Collection of strings used to control which fields are used to calculate score boosting
        /// </summary>
        [DataMember(Name = "boostedFields")]
        [JsonPropertyName("boostedFields")]
        public IEnumerable<string> BoostedFields { get; protected set; } = new string[] { "*" };

        public void ApplyClusterSettings(Cluster cluster)
        {
            ApplyClusterIndices(cluster.IndicesByType);
            ApplyQueryBoosts(cluster.Indices);
        }

        public void ApplyClusterIndices(ILookup<string, Index> indices)
        {
            var idx = indices[Type];
            var resp = Indices.ToList();
            resp.AddRange(
                idx
                    .Where(x =>
                        !resp.Any(z => z
                            .Equals(x.Name, StringComparison.OrdinalIgnoreCase)))
                    .Select(x => x.Name));
            Indices = resp.ToArray();
        }

        public void ApplyQueryBoosts(IEnumerable<Index> indices)
        {
            List<string> fields = new List<string>() { "*" };

            foreach(var idx in indices)
            {
                if (Indices.Contains(idx.Name))
                {
                    if (idx?.Fields is null) continue;
                    fields.AddRange(idx.Fields.SelectMany(x => x.AllBoostedFields));
                }
            }

            BoostedFields = fields.Distinct();
        }

        public AdvancedQueryCriteria(
            string type,
            IEnumerable<string> indices = null,
            IEnumerable<DefaultSortField> sortFields = null,
            IEnumerable<DefaultReturnField> returnFields = null,
            IEnumerable<DefaultFilterField> filterFields = null,
            IEnumerable<DefaultBucketField> bucketFields = null,
            int? skip = null,
            int? take = null) 
        {
            Type = type;
            Indices = indices?.ToArray() ?? Array.Empty<string>();
            _filterFields = filterFields;
            _sortFields = sortFields;
            _returnFields = returnFields;
            _bucketFields = bucketFields;
            Skip = skip;
            Take = take;
        }

        public SearchDescriptor<BaseDocument> GetSearchDescriptor()
        {
            var s = new SearchDescriptor<BaseDocument>()
                .Index(Indices)
                .Skip(Skip ?? 0)
                .Take(Take ?? 10)
                .Aggregations(a => BucketFields.GetBucketAggreagationDescriptor<BaseDocument>())
                .Query(q => FilterFields.GetQueryDesctiptor<BaseDocument>())
                .Source(t => ReturnFields.GetSourceFilterDescriptor<BaseDocument>())
                .Sort(s => SortFields.GetSortDescriptor<BaseDocument>());

            return s;
        }
    }

    public class AdvancedQueryCriteria<T> :
        ISeaqQueryCriteria<T>
    where T : BaseDocument
    {
        /// <summary>
        /// Specify which indices to query.  If empty or null, query will default to the default index for the provided type.
        /// </summary>
        [DataMember(Name = "indices")]
        [JsonPropertyName("indices")]
        public string[] Indices { get; protected set; }

        /// <summary>
        /// Used for paging.  Note that this is only deterministic if consistent sort fields are provided on each related query.
        /// </summary>
        [DataMember(Name = "skip")]
        [JsonPropertyName("skip")]
        public int? Skip { get; init; }

        /// <summary>
        /// Used for paging.  Note that this is only deterministic if consistent sort fields are provided on each related query.
        /// </summary>
        [DataMember(Name = "take")]
        [JsonPropertyName("take")]
        public int? Take { get; init; }

        /// <summary>
        /// Collection of FilterField objects used to construct the query
        /// </summary>
        [DataMember(Name = "filterFields")]
        [JsonPropertyName("filterFields")]
        private IEnumerable<DefaultFilterField> _filterFields { get; init; }
        public IEnumerable<IFilterField> FilterFields => _filterFields;

        /// <summary>
        /// Collection of SortField objects used to order the query results
        /// </summary>
        [DataMember(Name = "sortFields")]
        [JsonPropertyName("sortFields")]
        private IEnumerable<DefaultSortField> _sortFields { get; init; }
        public IEnumerable<ISortField> SortFields => _sortFields;

        /// <summary>
        /// Collection of ReturnField objects used to limit tthe fields included in the query results
        /// </summary>
        [DataMember(Name = "returnFields")]
        [JsonPropertyName("returnFields")]
        public IEnumerable<DefaultReturnField> _returnFields { get; init; }
        public IEnumerable<IReturnField> ReturnFields => _returnFields;

        /// <summary>
        /// Collection of BucketField objects used to control returned terms aggregations for further filtering
        /// </summary>
        [DataMember(Name = "bucketFields")]
        [JsonPropertyName("bucketFields")]
        public IEnumerable<DefaultBucketField> _bucketFields { get; init; }
        public IEnumerable<IBucketField> BucketFields => _bucketFields;

        /// <summary>
        /// Collection of strings used to control which fields are used to calculate score boosting
        /// </summary>
        [DataMember(Name = "boostedFields")]
        [JsonPropertyName("boostedFields")]
        public IEnumerable<string> BoostedFields { get; protected set; } = new string[] { "*" };

        public void ApplyClusterSettings(Cluster cluster)
        {
            ApplyClusterIndices(cluster.IndicesByType);
            ApplyQueryBoosts(cluster.Indices);
        }

        public virtual void ApplyClusterIndices(ILookup<string, Index> indices)
        {
            var idx = indices[typeof(T).FullName];
            var resp = Indices.ToList();
            resp.AddRange(
                idx
                    .Where(x =>
                        !resp.Any(z => z
                            .Equals(x.Name, StringComparison.OrdinalIgnoreCase)))
                    .Select(x => x.Name));
            Indices = resp.ToArray();
        }

        public void ApplyQueryBoosts(IEnumerable<Index> indices)
        {
            List<string> fields = new List<string>() { "*" };

            foreach (var idx in indices)
            {
                if (Indices.Contains(idx.Name))
                {
                    if (idx?.Fields is null) continue;
                    fields.AddRange(idx.Fields.SelectMany(x => x.AllBoostedFields));
                }
            }

            BoostedFields = fields.Distinct();
        }

        public AdvancedQueryCriteria(
            IEnumerable<string> indices = null,
            IEnumerable<DefaultSortField> sortFields = null,
            IEnumerable<DefaultReturnField> returnFields = null,
            IEnumerable<DefaultFilterField> filterFields = null,
            IEnumerable<DefaultBucketField> bucketFields = null,
            int? skip = null,
            int? take = null)
        {
            Indices = indices?.ToArray() ?? Array.Empty<string>();
            _filterFields = filterFields;
            _sortFields = sortFields;
            _returnFields = returnFields;
            _bucketFields = bucketFields;
            Skip = skip;
            Take = take;
        }

        public SearchDescriptor<T> GetSearchDescriptor()
        {
            var s = new SearchDescriptor<T>()
                .Index(Indices)
                .Skip(Skip ?? 0)
                .Take(Take ?? 10)
                .Aggregations(a => BucketFields.GetBucketAggreagationDescriptor<T>())
                .Query(q => FilterFields.GetQueryDesctiptor<T>())
                .Source(t => ReturnFields.GetSourceFilterDescriptor<T>())                
                .Sort(s => SortFields.GetSortDescriptor<T>());

            return s;
        }
    }
}
