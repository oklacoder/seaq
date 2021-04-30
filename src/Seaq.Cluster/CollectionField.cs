using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Seaq.Clusters{
    public class CollectionField 
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public double? Boost { get; set; }

        public string BoostedFieldName => $"{Name}^{Boost}";

        public bool? IsFilterable { get; set; }

        public bool? IncludeInResults { get; set; }

        public bool? IsIncludedByDefault => Constants.Fields.AlwaysReturnedFields.Any(x => x.Equals(Name, StringComparison.OrdinalIgnoreCase)) == true;

        public string Label { get; set; }


        public List<CollectionField> Fields { get; set; }

        public IEnumerable<string> FieldTree =>
            Fields == null ?
            new[] { this.Name } :
            new[] { this.Name }.Concat(Fields.SelectMany(x => x.FieldTree));

        public bool? IsBoostedField => Boost.HasValue && Boost != 0;

        public bool? IsIncludedField => IncludeInResults == true || IsIncludedByDefault == true;

        public bool? IsKeywordField => Name?.EndsWith(Constants.Fields.KeywordField, StringComparison.OrdinalIgnoreCase);

        public bool? IsSortField => Name?.EndsWith(Constants.Fields.SortField, StringComparison.OrdinalIgnoreCase);

        public bool? HasBoostedField => Fields?.Any(x => x.IsBoostedField == true) == true;

        public bool? HasIncludedField => Fields?.Any(x => x.IsIncludedField == true) == true;

        public bool? HasKeywordField => Fields?.Any(x => x.IsKeywordField == true) == true;

        public bool? HasSortField => Fields?.Any(x => x.IsSortField == true) == true;

        public IEnumerable<string> AllBoostedFields => new[] { this }.Concat(Fields).Where(x => x.IsBoostedField == true).Select(x => x.BoostedFieldName);

        public IEnumerable<string> AllIncludedFields => new[] { this }.Concat(Fields).Where(x => x.IsIncludedField == true).Select(x => x.Name);

        public IEnumerable<string> AllKeywordFields => new[] { this }.Concat(Fields).Where(x => x.IsKeywordField == true).Select(x => x.Name);

        public IEnumerable<string> AllSortFields => new[] { this }.Concat(Fields).Where(x => x.IsSortField == true).Select(x => x.Name);

        public CollectionField()
        {

        }
        public CollectionField(
            string name,
            string type,
            double? boost = null,
            bool? isFilterable = null,
            bool? includeInResults = null,
            string label = null,
            IEnumerable<CollectionField> fields = null)
        {
            Name = name;
            Type = type;
            Boost = boost;
            IsFilterable = isFilterable;
            IncludeInResults = includeInResults;
            Label = label;
            Fields = fields?.ToList() ?? new List<CollectionField>();
        }

    }

    public static class CollectionFieldExtensions
    {
        public static CollectionField ToCollectionField(
            this IProperty property,
            string parentFieldName = null)
        {
            var fieldName = 
                string.IsNullOrWhiteSpace(parentFieldName) ? 
                property.Name.Name : 
                string.Join(".", parentFieldName, property.Name.Name).Trim('.');

            return property switch
            {
                ObjectProperty prop => BuildField(prop, fieldName),
                TextProperty prop => BuildField(prop, fieldName),
                BooleanProperty prop => BuildField(prop, fieldName),
                NumberProperty prop => BuildField(prop, fieldName),
                DateProperty prop => BuildField(prop, fieldName),
                _ => BuildField(property, fieldName)
            };

        }

        private static CollectionField BuildField(
            IProperty property, 
            string fieldName)
        {
            return new CollectionField(
                fieldName,
                property.Type);
        }

        private static CollectionField BuildField(
            ObjectProperty property,
            string fieldName)
        {
            return new CollectionField(
                fieldName,
                (property as IProperty).Type,
                fields: property?.Properties?.Select(x => ToCollectionField(x.Value, fieldName))
            );
        }

        private static CollectionField BuildField(
            TextProperty property,
            string fieldName)
        {
            return new CollectionField(
                fieldName,
                (property as IProperty).Type,
                fields: property?.Fields?.Select(x => ToCollectionField(x.Value, fieldName))
            );
        }

        private static CollectionField BuildField(
            BooleanProperty property,
            string fieldName)
        {
            return new CollectionField(
                fieldName,
                (property as IProperty).Type,
                fields: property?.Fields?.Select(x => ToCollectionField(x.Value, fieldName))
            );
        }

        private static CollectionField BuildField(
            NumberProperty property,
            string fieldName)
        {
            return new CollectionField(
                fieldName,
                (property as IProperty).Type,
                fields: property?.Fields?.Select(x => ToCollectionField(x.Value, fieldName))
            );
        }

        private static CollectionField BuildField(
            DateProperty property,
            string fieldName)
        {
            return new CollectionField(
                fieldName,
                (property as IProperty).Type,
                fields: property?.Fields?.Select(x => ToCollectionField(x.Value, fieldName))
            );
        }
    }

}
