using System;
using System.Collections.Generic;
using System.Linq;

namespace Seaq.Clusters{
    public class CollectionField :
        ICollectionField
    {
        public string Name { get; }

        public string Type { get; }

        public double? Boost { get; }

        public string BoostedFieldName => $"{Name}^{Boost}";

        public bool? IsFilterable { get; }

        public bool? IncludeInResults { get; }

        public bool? IsIncludedByDefault => Constants.Fields.AlwaysReturnedFields.Any(x => x.Equals(Name, StringComparison.OrdinalIgnoreCase)) == true;

        public string Label { get; }


        private List<CollectionField> _fields { get; set; }
        public IEnumerable<ICollectionField> Fields => _fields;

        public IEnumerable<string> FieldTree =>
            Fields == null ?
            new[] { this.Name } :
            new[] { this.Name }.Concat(Fields.SelectMany(x => x.FieldTree));

        public bool? IsBoostedField => Boost.HasValue && Boost != 0;

        public bool? IsIncludedField => IncludeInResults == true || IsIncludedByDefault == true;

        public bool? IsKeywordField => Name.EndsWith(Constants.Fields.KeywordField, StringComparison.OrdinalIgnoreCase);

        public bool? IsSortField => Name.EndsWith(Constants.Fields.SortField, StringComparison.OrdinalIgnoreCase);

        public bool? HasBoostedField => Fields?.Any(x => x.IsBoostedField == true) == true;

        public bool? HasIncludedField => Fields?.Any(x => x.IsIncludedField == true) == true;

        public bool? HasKeywordField => Fields?.Any(x => x.IsKeywordField == true) == true;

        public bool? HasSortField => Fields?.Any(x => x.IsSortField == true) == true;

        public IEnumerable<string> AllBoostedFields => new[] { this }.Concat(Fields).Where(x => x.IsBoostedField == true).Select(x => x.BoostedFieldName);

        public IEnumerable<string> AllIncludedFields => new[] { this }.Concat(Fields).Where(x => x.IsIncludedField == true).Select(x => x.Name);

        public IEnumerable<string> AllKeywordFields => new[] { this }.Concat(Fields).Where(x => x.IsKeywordField == true).Select(x => x.Name);

        public IEnumerable<string> AllSortFields => new[] { this }.Concat(Fields).Where(x => x.IsSortField == true).Select(x => x.Name);


        public CollectionField(
            string name,
            string type,
            double? boost,
            bool? isFilterable,
            bool? includeInResults,
            string label,
            List<CollectionField> fields = null)
        {
            Name = name;
            Type = type;
            Boost = boost;
            IsFilterable = isFilterable;
            IncludeInResults = includeInResults;
            Label = label;
            _fields = fields ?? new List<CollectionField>();
        }

    }

}
