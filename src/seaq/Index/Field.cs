using System;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public interface IField
    {
        string Name { get; }
        string Type { get; }
        bool? IsKeywordField { get; }
        bool? IsSortField { get; }
        IEnumerable<IField>? Fields { get; }
    }
    public record KeywordField :
        IField
    {
        public string Name { get; init;}

        public string Type { get; init; }
        public bool? IsKeywordField => true;
        public bool? IsSortField => false;
        public IEnumerable<IField> Fields => null;
    }
    public record SortField :
        IField
    {
        public string Name { get; init; }

        public string Type { get; init; }
        public bool? IsKeywordField => false;
        public bool? IsSortField => true;
        public IEnumerable<IField> Fields => null;

    }
    

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

        public IEnumerable<string> FieldTree()
        {
            return Fields == null ?
                new[] { this.Name } :
                new[] { this.Name }.Concat(Fields.SelectMany(x => x.FieldTree()));
        }

        public bool? IsBoostedField => Boost.HasValue && Boost != 0;

        public bool? IsIncludedField() { return IncludeInResults == true || IsIncludedByDefault == true; }

        public bool? IsKeywordField() { return Name?.EndsWith(Constants.Fields.KeywordField, StringComparison.OrdinalIgnoreCase); }

        public bool? HasIncludedField() { return Fields?.Any(x => x.IsIncludedField() == true || x.HasIncludedField() == true) == true; }

        public bool? HasKeywordField() { return Fields?.Any(x => x.IsKeywordField() == true || x.HasKeywordField() == true) == true; }

        public bool? HasFilterField() { return Fields?.Any(x => x.IsFilterable == true || x.HasFilterField() == true) == true; }

        public IEnumerable<string> AllBoostedFields() { return new[] { this }.Concat(Fields)?.Where(x => x.IsBoostedField == true)?.Select(x => x.BoostedFieldName) ?? Array.Empty<string>(); }

        public IEnumerable<string> AllKeywordFields() { return new[] { this }.Concat(Fields)?.Where(x => x.IsKeywordField() == true)?.Select(x => x.Name) ?? Array.Empty<string>(); }



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
