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
        [DataMember(Name = "type")]
        [JsonPropertyName("type")]
        public string Type { get; init; }
        [DataMember(Name = "text")]
        [JsonPropertyName("text")]
        public string Text { get; init; }

        [DataMember(Name = "indices")]
        [JsonPropertyName("indices")]
        public string[] Indices { get; protected set; }

        [DataMember(Name = "skip")]
        [JsonPropertyName("skip")]
        public int? Skip { get; init; }

        [DataMember(Name = "take")]
        [JsonPropertyName("take")]
        public int? Take { get; init; }

        [DataMember(Name = "sortFields")]
        [JsonPropertyName("sortFields")]
        protected IEnumerable<DefaultSortField> _sortFields { get; init; }
        public IEnumerable<ISortField> SortFields => _sortFields;

        [DataMember(Name = "returnFields")]
        [JsonPropertyName("returnFields")]
        protected IEnumerable<DefaultReturnField> _returnFields { get; init; }
        public IEnumerable<IReturnField> ReturnFields => _returnFields;

        [DataMember(Name = "bucketFields")]
        [JsonPropertyName("bucketFields")]
        protected IEnumerable<DefaultBucketField> _bucketFields { get; init; }
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

        public SearchDescriptor<BaseDocument> GetSearchDescriptor()
        {
            var res = new SearchDescriptor<BaseDocument>()
                .Index(Indices)
                .Skip(Skip ?? 0)
                .Take(Take ?? 10)
                .Aggregations(a => BucketFields.GetBucketAggreagationDescriptor<BaseDocument>())
                .Source(t => ReturnFields.GetSourceFilterDescriptor<BaseDocument>())
                .Sort(x => SortFields.GetSortDescriptor<BaseDocument>())
                .Fields(f => f.Fields(BoostedFields.ToArray()))
                .Query(x => x.QueryString(q => q.Query($"{Text}*").DefaultField("*")));

            return res;
        }
    }
    [DataContract]
    public class SimpleQueryCriteria<T> :
        ISeaqQueryCriteria<T>
        where T : BaseDocument
    {
        [DataMember(Name = "text")]
        [JsonPropertyName("text")]
        public string Text { get; init; }

        [DataMember(Name = "indices")]
        [JsonPropertyName("indices")]
        public string[] Indices { get; protected set; }

        [DataMember(Name = "skip")]
        [JsonPropertyName("skip")]
        public int? Skip { get; init; }

        [DataMember(Name = "take")]
        [JsonPropertyName("take")]
        public int? Take { get; init; }

        [DataMember(Name = "sortFields")]
        [JsonPropertyName("sortFields")]
        protected IEnumerable<DefaultSortField> _sortFields { get; init; }
        public IEnumerable<ISortField> SortFields => _sortFields;

        [DataMember(Name = "returnFields")]
        [JsonPropertyName("returnFields")]
        protected IEnumerable<DefaultReturnField> _returnFields { get; init; }
        public IEnumerable<IReturnField> ReturnFields => _returnFields;

        [DataMember(Name = "bucketFields")]
        [JsonPropertyName("bucketFields")]
        protected IEnumerable<DefaultBucketField> _bucketFields { get; init; }
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

        public SearchDescriptor<T> GetSearchDescriptor()
        {
            var res = new SearchDescriptor<T>()
                .Index(Indices)
                .Skip(Skip ?? 0)
                .Take(Take ?? 10)
                .Aggregations(a => BucketFields.GetBucketAggreagationDescriptor<T>())
                .Source(t => ReturnFields.GetSourceFilterDescriptor<T>())
                .Sort(x => SortFields.GetSortDescriptor<T>())
                .Query(x => x.QueryString(q => q.Query($"{Text}*").DefaultField("*")));

            return res;
        }
    }
}
