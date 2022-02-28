using System;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class Field
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public double? Boost { get; set; }
        public string BoostedFieldName => $"{Name}^{Boost}";
        public bool? IsFilterable { get; set; }
        public int? DisplayOrder { get; set; }

        public bool? IncludeInResults { get; set; }

        public bool? IsIncludedByDefault => Constants.Fields.AlwaysReturnedFields.Any(x => x.Equals(Name, StringComparison.OrdinalIgnoreCase)) == true;

        public string Label { get; set; }

        public IEnumerable<Field> Fields { get; set; } = Array.Empty<Field>();

        public IEnumerable<string> FieldTree =>
            Fields == null ?
            new[] { this.Name } :
            new[] { this.Name }.Concat(Fields.SelectMany(x => x.FieldTree));

        public bool? IsBoostedField => Boost.HasValue && Boost != 0;

        public bool? IsIncludedField => IncludeInResults == true || IsIncludedByDefault == true;

        public bool? IsKeywordField => Name?.EndsWith(Constants.Fields.KeywordField, StringComparison.OrdinalIgnoreCase);

        public bool? IsSortField => Name?.EndsWith(Constants.Fields.SortField, StringComparison.OrdinalIgnoreCase);

        public bool? HasBoostedField => Fields?.Any(x => x.IsBoostedField == true || x.HasBoostedField == true) == true;

        public bool? HasIncludedField => Fields?.Any(x => x.IsIncludedField == true || x.HasIncludedField == true) == true;

        public bool? HasKeywordField => Fields?.Any(x => x.IsKeywordField == true || x.HasKeywordField == true) == true;

        public bool? HasSortField => Fields?.Any(x => x.IsSortField == true || x.HasSortField == true) == true;
        public bool? HasFilterField => Fields?.Any(x => x.IsFilterable == true || x.HasFilterField == true) == true;

        public IEnumerable<string> AllBoostedFields => new[] { this }.Concat(Fields)?.Where(x => x.IsBoostedField == true)?.Select(x => x.BoostedFieldName) ?? Array.Empty<string>();

        public IEnumerable<string> AllIncludedFields => new[] { this }.Concat(Fields)?.Where(x => x.IsIncludedField == true)?.Select(x => x.Name) ?? Array.Empty<string>();

        public IEnumerable<string> AllKeywordFields => new[] { this }.Concat(Fields)?.Where(x => x.IsKeywordField == true)?.Select(x => x.Name) ?? Array.Empty<string>();

        public IEnumerable<string> AllSortFields => new[] { this }.Concat(Fields)?.Where(x => x.IsSortField == true)?.Select(x => x.Name) ?? Array.Empty<string>();

        public Field()
        {

        }
        public Field(
            string name)
        {
            Name = name;
        }
        public Field(
            string name,
            string type,
            double? boost = null,
            bool? isFilterable = null,
            bool? includeInResults = null,
            string label = null,
            IEnumerable<Field> fields = null)
        {
            Name = name;
            Type = type;
            Boost = boost;
            IsFilterable = isFilterable;
            IncludeInResults = includeInResults;
            Label = label;
            Fields = fields?.ToList() ?? new List<Field>();
        }
    }
}
