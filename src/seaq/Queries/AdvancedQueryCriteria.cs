using Nest;
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
        public string Type { get; private set; }

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
        public int? Skip { get; private set; }

        /// <summary>
        /// Used for paging.  Note that this is only deterministic if consistent sort fields are provided on each related query.
        /// </summary>
        [DataMember(Name = "take")]
        [JsonPropertyName("take")]
        public int? Take { get; private set; }

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
        private IEnumerable<DefaultBucketField> _bucketFields { get; set; }
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

        public void ApplyClusterSettings(Cluster cluster)
        {
            ApplyClusterIndices(cluster);
            ApplyQueryBoosts(cluster.Indices);
            ApplyDefaultSourceFilter(cluster);
            ApplyDefaultBuckets(cluster);
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
            if (string.IsNullOrWhiteSpace(Type))
            {
                idx = cluster.Indices.Where(x =>
                    x.IsHidden is not true &&
                    x.ReturnInGlobalSearch is true);
            }
            else
            {
                idx = cluster.IndicesByType[Type];
                idx = idx.Where(x => x.IsHidden is not true);
            }
            DeprecatedIndexTargets = idx.Where(x => x.IsDeprecated).Select(x => $"{x.Name} is deprecated - {x.DeprecationMessage}");
            Indices = idx
                .Select(x =>
                {
                    //it really feels like this needs more than this, but i can't put my finger on what
                    if (string.IsNullOrWhiteSpace(x.IndexAsType))
                        return x.Name;

                    var byType = cluster.IndicesByType[x.IndexAsType];
                    if (byType?.Any() is not true)
                        throw new InvalidOperationException($"Index {x.Name} expects to query index of type {x.IndexAsType}, but no index exists on the cluster matching to that type.");
                    return byType?.FirstOrDefault()?.Name;
                })
                .ToArray();
            if (Indices?.Any() is not true)
            {
                throw new InvalidOperationException($"No indices could be identified for type {Type}.  Query could not be processed.  " +
                    $"Ensure that either an index exists on your seaq cluster fo this type, or that you have specified an explicit type in your query definition.");
            }
        }
        internal void ApplyQueryBoosts(IEnumerable<Index> indices)
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
        internal void ApplyDefaultSourceFilter(Cluster cluster)
        {
            if (_returnFields?.Any() is true)
                return;

            var indices = cluster.Indices.Where(x => Indices.Any(z => z.Equals(x.Name, StringComparison.OrdinalIgnoreCase)));

            var flat = indices
                .SelectMany(x =>
                    x.Fields.Where(x => x.IsIncludedField is true || x.HasIncludedField is true))
                .SelectMany(x =>
                    new[] { x }.Concat(x.Fields));

            _returnFields = flat
                .Where(x => x.IsIncludedField is true)
                .SelectMany(x =>
                    x.FieldTree?
                        .Select(z => new DefaultReturnField(z)));
        }
        internal void ApplyDefaultBuckets(Cluster cluster)
        {
            if (_bucketFields?.Any() is true)
                return;

            var indices = cluster.Indices.Where(x => Indices.Any(z => z.Equals(x.Name, StringComparison.OrdinalIgnoreCase)));

            var flat = indices
                .SelectMany(x =>
                    x.Fields.Where(x => x.IsFilterable is true || x.HasFilterField is true))
                .SelectMany(x =>
                    new[] { x }.Concat(x.Fields));

            _bucketFields = flat
                .Where(x => x.IsFilterable is true)
                .SelectMany(x =>
                    x.HasKeywordField is true ?
                    x.AllKeywordFields.Select(z => new DefaultBucketField(z)) :
                    new[] { new DefaultBucketField(x.Name) }
                );
        }

        public AdvancedQueryCriteria(
            string text = null,
            string type = null,
            IEnumerable<string> indices = null,
            IEnumerable<DefaultSortField> sortFields = null,
            IEnumerable<DefaultReturnField> returnFields = null,
            IEnumerable<DefaultFilterField> filterFields = null,
            IEnumerable<DefaultBucketField> bucketFields = null,
            int? skip = null,
            int? take = null) 
        {
            Type = type;
            Text = text;
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
                .Query(x => x.GetQueryContainerDescriptor(Text, FilterFields))
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
        public int? Skip { get; private set; }

        /// <summary>
        /// Used for paging.  Note that this is only deterministic if consistent sort fields are provided on each related query.
        /// </summary>
        [DataMember(Name = "take")]
        [JsonPropertyName("take")]
        public int? Take { get; private set; }

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

        public void ApplyClusterSettings(Cluster cluster)
        {
            ApplyClusterIndices(cluster);
            ApplyQueryBoosts(cluster.Indices);
            ApplyDefaultSourceFilter(cluster);
            ApplyDefaultBuckets(cluster);
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
            Indices = idx
                .Select(x =>
                {
                    //it really feels like this needs more than this, but i can't put my finger on what
                    if (string.IsNullOrWhiteSpace(x.IndexAsType))
                        return x.Name;

                    var byType = cluster.IndicesByType[x.IndexAsType];
                    if (byType?.Any() is not true)
                        throw new InvalidOperationException($"Index {x.Name} expects to query index of type {x.IndexAsType}, but no index exists on the cluster matching to that type.");
                    return byType?.FirstOrDefault()?.Name;
                })
                .ToArray();
            if (Indices?.Any() is not true)
            {
                throw new InvalidOperationException($"No indices could be identified for type {typeof(T).FullName}.  Query could not be processed.  " +
                    $"Ensure that either an index exists on your seaq cluster fo this type, or that you have specified an explicit type in your query definition.");
            }
        }
        internal void ApplyQueryBoosts(IEnumerable<Index> indices)
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
        internal void ApplyDefaultSourceFilter(Cluster cluster)
        {
            if (_returnFields?.Any() is true)
                return;

            var indices = cluster.Indices.Where(x => Indices.Any(z => z.Equals(x.Name, StringComparison.OrdinalIgnoreCase)));

            var flat = indices
                .SelectMany(x => 
                    x.Fields.Where(x => x.IsIncludedField is true || x.HasIncludedField is true))
                .SelectMany(x => 
                    new[] { x }.Concat(x.Fields));

            _returnFields = flat
                .Where(x => x.IsIncludedField is true)
                .SelectMany(x => 
                    x.FieldTree?
                        .Select(z => new DefaultReturnField(z)));
        }
        internal void ApplyDefaultBuckets(Cluster cluster)
        {
            if (_bucketFields?.Any() is true)
                return;

            var indices = cluster.Indices.Where(x => Indices.Any(z => z.Equals(x.Name, StringComparison.OrdinalIgnoreCase)));

            var flat = indices
                .SelectMany(x =>
                    x.Fields.Where(x => x.IsFilterable is true || x.HasFilterField is true))
                .SelectMany(x =>
                    new[] { x }.Concat(x.Fields));

            _bucketFields = flat
                .Where(x => x.IsFilterable is true)
                .SelectMany(x =>
                    x.HasKeywordField is true ?
                    x.AllKeywordFields.Select(z => new DefaultBucketField(z)) :
                    new[] { new DefaultBucketField(x.Name) }
                );
        }

        public AdvancedQueryCriteria(
            string text = null,
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
            Text = text;
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
                .Query(x => x.GetQueryContainerDescriptor(Text, FilterFields))
                .Source(t => ReturnFields.GetSourceFilterDescriptor<T>())                
                .Sort(s => SortFields.GetSortDescriptor<T>());

            return s;
        }
    }
}
