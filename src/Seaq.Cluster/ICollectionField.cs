using System.Collections.Generic;

namespace Seaq.Clusters{
    public interface ICollectionField
    {
        public string Name { get; }
        public string Type { get; }
        public double? Boost { get; }
        public string BoostedFieldName { get; }
        public bool? IsFilterable { get; }
        public bool? IncludeInResults { get; }
        public bool? IsIncludedByDefault { get; }
        public string Label { get; }
        public IEnumerable<ICollectionField> Fields { get; }
        public IEnumerable<string> FieldTree { get; }
        public bool? IsBoostedField { get; }
        public bool? IsIncludedField { get; }
        public bool? IsKeywordField { get; }
        public bool? IsSortField { get; }
        public bool? HasBoostedField { get; }
        public bool? HasIncludedField { get; }
        public bool? HasKeywordField { get; }
        public bool? HasSortField { get; }
        public IEnumerable<string> AllBoostedFields { get; }
        public IEnumerable<string> AllIncludedFields { get; }
        public IEnumerable<string> AllKeywordFields { get; }
        public IEnumerable<string> AllSortFields { get; }
    }

}
