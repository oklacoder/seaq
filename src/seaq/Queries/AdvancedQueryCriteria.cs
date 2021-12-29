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
        [DataMember(Name = "type")]
        [JsonPropertyName("type")]
        public string Type { get; init; }
        [DataMember(Name = "indices")]
        [JsonPropertyName("indices")]
        public string[] Indices { get; protected set; }


        [DataMember(Name = "skip")]
        [JsonPropertyName("skip")]
        public int? Skip { get; init; }

        [DataMember(Name = "take")]
        [JsonPropertyName("take")]
        public int? Take { get; init; }

        [DataMember(Name = "filterFields")]
        [JsonPropertyName("filterFields")]
        private IEnumerable<DefaultFilterField> _filterFields { get; init; }
        public IEnumerable<IFilterField> FilterFields => _filterFields;

        [DataMember(Name = "sortFields")]
        [JsonPropertyName("sortFields")]
        private IEnumerable<DefaultSortField> _sortFields { get; init; }
        public IEnumerable<ISortField> SortFields => _sortFields;

        [DataMember(Name = "returnFields")]
        [JsonPropertyName("returnFields")]
        public IEnumerable<DefaultReturnField> _returnFields { get; init; }
        public IEnumerable<IReturnField> ReturnFields => _returnFields;

        [DataMember(Name = "bucketFields")]
        [JsonPropertyName("bucketFields")]
        public IEnumerable<DefaultBucketField> _bucketFields { get; init; }
        public IEnumerable<IBucketField> BucketFields => _bucketFields;

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
        [DataMember(Name = "indices")]
        [JsonPropertyName("indices")]
        public string[] Indices { get; protected set; }


        [DataMember(Name = "skip")]
        [JsonPropertyName("skip")]
        public int? Skip { get; init; }

        [DataMember(Name = "take")]
        [JsonPropertyName("take")]
        public int? Take { get; init; }

        [DataMember(Name = "filterFields")]
        [JsonPropertyName("filterFields")]
        private IEnumerable<DefaultFilterField> _filterFields { get; init; }
        public IEnumerable<IFilterField> FilterFields => _filterFields;

        [DataMember(Name = "sortFields")]
        [JsonPropertyName("sortFields")]
        private IEnumerable<DefaultSortField> _sortFields { get; init; }
        public IEnumerable<ISortField> SortFields => _sortFields;

        [DataMember(Name = "returnFields")]
        [JsonPropertyName("returnFields")]
        public IEnumerable<DefaultReturnField> _returnFields { get; init; }
        public IEnumerable<IReturnField> ReturnFields => _returnFields;

        [DataMember(Name = "bucketFields")]
        [JsonPropertyName("bucketFields")]
        public IEnumerable<DefaultBucketField> _bucketFields { get; init; }
        public IEnumerable<IBucketField> BucketFields => _bucketFields;

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
