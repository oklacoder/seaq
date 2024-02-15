using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace seaq
{
    [DataContract]
    public class SimpleQueryCriteria :
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
        public int? Skip { get; set; }

        /// <summary>
        /// Used for paging.  Note that this is only deterministic if consistent sort fields are provided on each related query.
        /// </summary>
        [DataMember(Name = "take")]
        [JsonPropertyName("take")]
        public int? Take { get; set; }

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
        private IEnumerable<DefaultBucketField> _bucketFields { get; set; }
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
        public SimpleQueryCriteria() { }

        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        public SimpleQueryCriteria(
            string type = null,
            string text = null,
            IEnumerable<string> indices = null,
            int? skip = null,
            int? take = null,
            IEnumerable<DefaultSortField> sortFields = null,
            IEnumerable<DefaultBucketField> bucketFields = null,
            IEnumerable<DefaultReturnField> returnFields = null,
            bool overrideClusterSettings = false)
        {
            Type = type;
            Text = text;
            Indices = indices ?? Array.Empty<string>();
            Skip = skip;
            Take = take;
            _sortFields = sortFields;
            _bucketFields = bucketFields;
            _returnFields = returnFields;
            OverrideClusterSettings = overrideClusterSettings;
        }

        public SimpleQueryCriteria<T> GetAsTyped<T>()
            where T : BaseDocument
        {
            return new SimpleQueryCriteria<T>(
                Text, Indices, Skip, Take, _sortFields, _bucketFields, _returnFields);
        }

        public SearchDescriptor<BaseDocument> GetSearchDescriptor()
        {
            var res = new SearchDescriptor<BaseDocument>()
                .Index(Indices?.ToArray() ?? Array.Empty<string>())
                .Skip(Skip ?? 0)
                .Take(Take ?? 10)
                .Aggregations(a => BucketFields.GetBucketAggreagationDescriptor<BaseDocument>())
                .Source(t => ReturnFields.GetSourceFilterDescriptor<BaseDocument>())
                .Sort(x => SortFields.GetSortDescriptor<BaseDocument>())
                .Query(x => x.GetQueryContainerDescriptor(Text, boostFields: BoostedFields));

            return res;
        }

        public void UpdatePaging(
            int skip,
            int take)
        {
            this.Skip = skip;
            this.Take = take;
        }

        public void ApplyClusterSettings(Cluster cluster)
        {
            this.ApplyClusterIndices(cluster);
            if (OverrideClusterSettings is not true)
            {
                ApplyQueryBoosts(cluster.Indices);
                ApplyDefaultSourceFilter(cluster);
                ApplyDefaultBuckets(cluster);
            }
        }

        internal void ApplyQueryBoosts(IEnumerable<Index> indices)
        {
            List<string> fields = new List<string>() { "*" };

            foreach (var idx in indices)
            {
                if (idx?.Fields is null) continue;
                if (Indices.Contains(idx.Name))
                {
                    fields.AddRange(idx.Fields.SelectMany(x => x.AllBoostedFields()));
                }
            }
            if (fields?.Any() is not true)
                fields.Add("id^1");

            BoostedFields = fields.Distinct();
        }
        internal void ApplyDefaultSourceFilter(Cluster cluster)
        {
            if (_returnFields?.Any() is true)
                return;

            var indices = cluster.Indices.Where(x => Indices.Any(z => z.Equals(x.Name, StringComparison.OrdinalIgnoreCase)));

            var flat = indices
                .SelectMany(x =>
                    x.Fields
                        .Where(x => x.IsIncludedByDefault is not true)
                        .Where(x => x.IsIncludedField() is true || x.HasIncludedField() is true))
                .SelectMany(x =>
                    new[] { x }.Concat(x.Fields));

            if (flat?.Any() is true)
                _returnFields = flat
                    .Where(x => x.IsIncludedField() is true)
                    .SelectMany(x =>
                        x.FieldTree()?
                            .Select(z => new DefaultReturnField(z)));
        }
        internal void ApplyDefaultBuckets(Cluster cluster)
        {
            if (_bucketFields?.Any() is true)
                return;

            var indices = cluster.Indices.Where(x => Indices.Any(z => z.Equals(x.Name, StringComparison.OrdinalIgnoreCase)));

            var flat = indices
                .SelectMany(x =>
                    x.Fields.Where(x => x.IsFilterable is true || x.HasFilterField() is true))
                .SelectMany(x =>
                    new[] { x }.Concat(x.Fields));

            _bucketFields = flat
                .Where(x => x.IsFilterable is true)
                .SelectMany(x =>
                    x.HasKeywordField() is true ?
                    x.AllKeywordFields().Select(z => new DefaultBucketField(z)) :
                    new[] { new DefaultBucketField(x.Name) }
                );
        }

    }
    [DataContract]
    public class SimpleQueryCriteria<T> :
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
        public int? Skip { get; set; }

        /// <summary>
        /// Used for paging.  Note that this is only deterministic if consistent sort fields are provided on each related query.
        /// </summary>
        [DataMember(Name = "take")]
        [JsonPropertyName("take")]
        public int? Take { get; set; }

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
        private IEnumerable<DefaultBucketField> _bucketFields { get; set; }
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
        public SimpleQueryCriteria() { }

        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        public SimpleQueryCriteria(
            string text,
            IEnumerable<string> indices = null,
            int? skip = null,
            int? take = null,
            IEnumerable<DefaultSortField> sortFields = null,
            IEnumerable<DefaultBucketField> bucketFields = null,
            IEnumerable<DefaultReturnField> returnFields = null,
            bool overrideClusterSettings = false)
        {
            Text = text;
            Indices = indices ?? Array.Empty<string>();
            Skip = skip;
            Take = take;
            _sortFields = sortFields;
            _bucketFields = bucketFields;
            _returnFields = returnFields;
            OverrideClusterSettings = overrideClusterSettings;
        }

        public SearchDescriptor<T> GetSearchDescriptor()
        {
            var res = new SearchDescriptor<T>()
                .Index(Indices?.ToArray() ?? Array.Empty<string>())
                .Skip(Skip ?? 0)
                .Take(Take ?? 10)
                .Aggregations(a => BucketFields.GetBucketAggreagationDescriptor<T>())
                .Source(t => ReturnFields.GetSourceFilterDescriptor<T>())
                .Sort(x => SortFields.GetSortDescriptor<T>())
                .Query(x => x.GetQueryContainerDescriptor(Text, boostFields: BoostedFields));

            return res;
        }

        public void UpdatePaging(
            int skip,
            int take)
        {
            this.Skip = skip;
            this.Take = take;
        }

        public void ApplyClusterSettings(Cluster cluster)
        {
            this.ApplyClusterIndices(cluster);
            if (OverrideClusterSettings is not true)
            {
                ApplyQueryBoosts(cluster.Indices);
                ApplyDefaultSourceFilter(cluster);
                ApplyDefaultBuckets(cluster);
            }
        }

        internal void ApplyQueryBoosts(IEnumerable<Index> indices)
        {
            List<string> fields = new List<string>() { "*" };

            foreach (var idx in indices)
            {
                if (idx?.Fields is null) continue;
                if (Indices.Contains(idx.Name))
                {
                    fields.AddRange(idx.Fields.SelectMany(x => x.AllBoostedFields()));
                }
            }
            if (fields?.Any() is not true)
                fields.Add("id^1");

            BoostedFields = fields.Distinct();
        }
        internal void ApplyDefaultSourceFilter(Cluster cluster)
        {
            if (_returnFields?.Any() is true)
                return;

            var indices = cluster.Indices.Where(x => Indices.Any(z => z.Equals(x.Name, StringComparison.OrdinalIgnoreCase)));

            var flat = indices
                .SelectMany(x =>
                    x.Fields
                        .Where(x => x.IsIncludedByDefault is not true)
                        .Where(x => x.IsIncludedField() is true || x.HasIncludedField() is true))
                .SelectMany(x =>
                    new[] { x }.Concat(x.Fields));

            if (flat?.Any() is true)
                _returnFields = flat
                    .Where(x => x.IsIncludedField() is true)
                    .SelectMany(x =>
                        x.FieldTree()?
                            .Select(z => new DefaultReturnField(z)));
        }
        internal void ApplyDefaultBuckets(Cluster cluster)
        {
            if (_bucketFields?.Any() is true)
                return;

            var indices = cluster.Indices.Where(x => Indices.Any(z => z.Equals(x.Name, StringComparison.OrdinalIgnoreCase)));

            var flat = indices
                .SelectMany(x =>
                    x.Fields.Where(x => x.IsFilterable is true || x.HasFilterField() is true))
                .SelectMany(x =>
                    new[] { x }.Concat(x.Fields));

            _bucketFields = flat
                .Where(x => x.IsFilterable is true)
                .SelectMany(x =>
                    x.HasKeywordField() is true ?
                    x.AllKeywordFields().Select(z => new DefaultBucketField(z)) :
                    new[] { new DefaultBucketField(x.Name) }
                );
        }

    }
}
