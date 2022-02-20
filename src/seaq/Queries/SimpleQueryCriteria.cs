using Elastic.Clients.Elasticsearch;
using Serilog;
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
        public string Type { get; init; }

        /// <summary>
        /// Query text
        /// </summary>
        [DataMember(Name = "text")]
        [JsonPropertyName("text")]
        public string Text { get; init; }

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
        /// Collection of SortField objects used to order the query results
        /// </summary>
        [DataMember(Name = "sortFields")]
        [JsonPropertyName("sortFields")]
        protected IEnumerable<DefaultSortField> _sortFields { get; init; }
        public IEnumerable<ISortField> SortFields => _sortFields;

        /// <summary>
        /// Collection of ReturnField objects used to limit tthe fields included in the query results
        /// </summary>
        [DataMember(Name = "returnFields")]
        [JsonPropertyName("returnFields")]
        protected IEnumerable<DefaultReturnField> _returnFields { get; init; }
        public IEnumerable<IReturnField> ReturnFields => _returnFields;

        /// <summary>
        /// Collection of BucketField objects used to control returned terms aggregations for further filtering
        /// </summary>
        [DataMember(Name = "bucketFields")]
        [JsonPropertyName("bucketFields")]
        protected IEnumerable<DefaultBucketField> _bucketFields { get; init; }
        public IEnumerable<IBucketField> BucketFields => _bucketFields;

        /// <summary>
        /// Collection of strings used to control which fields are used to calculate score boosting
        /// </summary>
        [DataMember(Name = "boostedFields")]
        [JsonPropertyName("boostedFields")]
        public IEnumerable<string> BoostedFields { get; protected set; } = new string[] { "*" };

        /// <summary>
        /// Indexes targeted by this query that are marked as "deprecated" on the containing cluster
        /// </summary>
        [DataMember(Name = "deprecatedIndexTargets")]
        [JsonPropertyName("deprecatedIndexTargets")]
        public IEnumerable<string> DeprecatedIndexTargets { get; protected set; } = Enumerable.Empty<string>();

        public void ApplyClusterSettings(Cluster cluster)
        {
            ApplyClusterIndices(cluster);
            ApplyQueryBoosts(cluster.Indices);
        }

        public void ApplyClusterIndices(Cluster cluster)
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
                DeprecatedIndexTargets = idx.Where(x => x.IsDeprecated).Select(x => $"{x.Name} is deprecated - {x.DeprecationMessage}");
                idx = idx.Where(x => x.IsHidden is not true);
            }
            Indices = idx.Select(x => x.Name).ToArray();
        }

        public void ApplyQueryBoosts(IEnumerable<Index> indices)
        {
            List<string> fields = new List<string>() { "*" };

            foreach (var idx in indices)
            {
                if (idx?.Fields is null) continue;
                if (Indices.Contains(idx.Name))
                {
                    fields.AddRange(idx.Fields.SelectMany(x => x.AllBoostedFields));
                }
            }

            BoostedFields = fields.Distinct();
        }
        public SimpleQueryCriteria(
            string type,
            string text,
            string[] indices = null,
            int? skip = null,
            int? take = null,
            IEnumerable<DefaultSortField> sortFields = null,
            IEnumerable<DefaultBucketField> bucketFields = null,
            IEnumerable<DefaultReturnField> returnFields = null)
        {
            Type = type;
            Text = text;
            Indices = indices ?? Array.Empty<string>();
            Skip = skip;
            Take = take;
            _sortFields = sortFields;
            _bucketFields = bucketFields;
            _returnFields = returnFields;
        }

        public SimpleQueryCriteria<T> GetAsTyped<T>()
            where T : BaseDocument
        {
            return new SimpleQueryCriteria<T>(
                Text, Indices, Skip, Take, _sortFields, _bucketFields, _returnFields);
        }

        public SearchRequestDescriptor<BaseDocument> GetSearchDescriptor()
        {
            var res = new SearchRequestDescriptor<BaseDocument>()
                .Index(Indices)
                .From(Skip ?? 0)
                .Size(Take ?? 10)
                .Aggregations(a => BucketFields.GetBucketAggreagationDescriptor<BaseDocument>())
                //.Source(t => ReturnFields.GetSourceFilterDescriptor<BaseDocument>())
                .Sort(x => SortFields.GetSortDescriptor<BaseDocument>())
                //.Fields(f => f.Fields(BoostedFields.ToArray()))
                .Query(x => x.QueryString(q => q.Query($"{Text}*").DefaultField("*")));

            Log.Warning("Source filtering is currently disabled.");
            Log.Warning("Field-level score boosting is currently disabled.");

            return res;
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
        public string Text { get; init; }

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
        /// Collection of SortField objects used to order the query results
        /// </summary>
        [DataMember(Name = "sortFields")]
        [JsonPropertyName("sortFields")]
        protected IEnumerable<DefaultSortField> _sortFields { get; init; }
        public IEnumerable<ISortField> SortFields => _sortFields;

        /// <summary>
        /// Collection of ReturnField objects used to limit tthe fields included in the query results
        /// </summary>
        [DataMember(Name = "returnFields")]
        [JsonPropertyName("returnFields")]
        protected IEnumerable<DefaultReturnField> _returnFields { get; init; }
        public IEnumerable<IReturnField> ReturnFields => _returnFields;

        /// <summary>
        /// Collection of BucketField objects used to control returned terms aggregations for further filtering
        /// </summary>
        [DataMember(Name = "bucketFields")]
        [JsonPropertyName("bucketFields")]
        protected IEnumerable<DefaultBucketField> _bucketFields { get; init; }
        public IEnumerable<IBucketField> BucketFields => _bucketFields;

        /// <summary>
        /// Collection of strings used to control which fields are used to calculate score boosting
        /// </summary>
        [DataMember(Name = "boostedFields")]
        [JsonPropertyName("boostedFields")]
        public IEnumerable<string> BoostedFields { get; protected set; } = new string[] { "*" };

        /// <summary>
        /// Indexes targeted by this query that are marked as "deprecated" on the containing cluster
        /// </summary>
        [DataMember(Name = "deprecatedIndexTargets")]
        [JsonPropertyName("deprecatedIndexTargets")]
        public IEnumerable<string> DeprecatedIndexTargets { get; protected set; } = Enumerable.Empty<string>();

        public void ApplyClusterSettings(Cluster cluster)
        {
            ApplyClusterIndices(cluster);
            ApplyQueryBoosts(cluster.Indices);
        }

        public virtual void ApplyClusterIndices(Cluster cluster)
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
                idx = idx.Where(x => x.IsHidden is not true);
            }
            DeprecatedIndexTargets = idx.Where(x => x.IsDeprecated).Select(x => $"{x.Name} is deprecated - {x.DeprecationMessage}");
            Indices = idx.Select(x => x.Name).ToArray();
        }

        public void ApplyQueryBoosts(IEnumerable<Index> indices)
        {
            List<string> fields = new List<string>() { "*" };

            foreach (var idx in indices)
            {
                if (idx?.Fields is null) continue;
                if (Indices.Contains(idx.Name))
                {
                    fields.AddRange(idx.Fields.SelectMany(x => x.AllBoostedFields));
                }
            }

            BoostedFields = fields.Distinct();
        }

        public SimpleQueryCriteria() { }
        public SimpleQueryCriteria(
            string text,
            string[] indices = null,
            int? skip = null,
            int? take = null,
            IEnumerable<DefaultSortField> sortFields = null,
            IEnumerable<DefaultBucketField> bucketFields = null,
            IEnumerable<DefaultReturnField> returnFields = null)
        {
            Text = text;
            Indices = indices ?? Array.Empty<string>();
            Skip = skip;
            Take = take;
            _sortFields = sortFields;
            _bucketFields = bucketFields;
            _returnFields = returnFields;
        }

        public SearchRequestDescriptor<T> GetSearchDescriptor()
        {
            var res = new SearchRequestDescriptor<T>()
                .Index(Indices)
                .From(Skip ?? 0)
                .Size(Take ?? 10)
                .Aggregations(a => BucketFields.GetBucketAggreagationDescriptor<T>())
                //.Source(t => ReturnFields.GetSourceFilterDescriptor<T>())
                .Sort(x => SortFields.GetSortDescriptor<T>())
                .Query(x => x.QueryString(q => q.Query($"{Text}*").DefaultField("*")));
            Log.Warning("Source filtering is currently disabled.");
            return res;
        }
    }
}
