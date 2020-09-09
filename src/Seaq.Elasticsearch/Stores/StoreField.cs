using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Seaq.Elasticsearch.Stores
{
    [DataContract]
    public class StoreField
    {
        [DataMember(Name = nameof(Name))]
        public string Name { get; }

        [DataMember(Name = nameof(Type))]
        public string Type { get; }

        [DataMember(Name = nameof(Boost))]
        public double? Boost { get; }

        [DataMember(Name = nameof(IsFilterable))]
        public bool? IsFilterable { get; }

        [DataMember(Name = nameof(Label))]
        public string Label { get; }

        [DataMember(Name = nameof(Fields))]
        public StoreField[] Fields { get; }

        public string[] FieldTree => Fields == null ?
            new[] { this.Name } :
            new[] { this.Name }.Concat(Fields?.Select(x => $"{x.Name}")).ToArray();

        public bool? HasKeywordField => Fields?.Any(p => p.IsKeywordField == true);

        public bool? HasSortField => Fields?.Any(p => p.IsSortField == true);

        public bool? IsKeywordField => Name.Contains(WellKnownKeys.Fields.KeywordField, System.StringComparison.OrdinalIgnoreCase);

        public bool? IsSortField => Name.Contains(WellKnownKeys.Fields.LowerField, System.StringComparison.OrdinalIgnoreCase);

        public string[] GetKeywordFieldNames => 
            IsKeywordField == true ? 
                new[] { Name } : 
                Fields?.SelectMany(x => x?.GetKeywordFieldNames)?.ToArray() ?? new string[] { };

        public string[] GetSortFieldNames =>
            IsSortField == true ?
                new[] { Name } :
                Fields?.SelectMany(x => x?.GetSortFieldNames)?.ToArray() ?? new string[] { };

        public string GetBoostedFieldName => Boost.HasValue ? $"{Name}^{Boost}" : Name;

        //This constructor, with the array of fields instead of the ienum,
        //is required to make messagepack happy.  it seems like you should be
        //able to delete it, but you'll break stuff.
        [JsonConstructor]
        public StoreField(
            string name,
            string type,
            StoreField[] fields,
            string label,
            double? boost,
            bool? isFilterable)
        {
            Name = name;
            Type = type;
            Boost = boost;
            IsFilterable = isFilterable;
            Label = label;
            Fields = fields;
        }

        public StoreField(
            string name,
            string type,
            IEnumerable<StoreField> fields,
            string label = null,
            double? boost = null,
            bool? isFilterable = true)
        {
            Name = name;
            Type = type;
            Boost = boost;
            IsFilterable = isFilterable;
            Label = label;
            Fields = fields?.ToArray();
        }

    }
}
