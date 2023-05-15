using Nest;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public static class FieldExtensions
    {
        public static void Merge(
            this IEnumerable<Field> target,
            IEnumerable<Field> source)
        {
            if (target is null) return;

            foreach(var tgt in target)
            {
                var src = source.FirstOrDefault(x => x.Name.Equals(tgt.Name));
                if (src is not null)
                {
                    if (tgt.Boost.HasValue)
                        src.Boost = tgt.Boost;
                    if (tgt.IsFilterable.HasValue)
                        src.IsFilterable = tgt.IsFilterable;
                    if (tgt.IncludeInResults.HasValue)
                        src.IncludeInResults = tgt.IncludeInResults;
                    if (!string.IsNullOrWhiteSpace(tgt.Label))
                        src.Label = tgt.Label;

                    if (tgt.Fields?.Any() == src.Fields?.Any() == true)
                        tgt.Fields.Merge(src.Fields);
                }
            }
        }
        public static void Merge(
            this Field target,
            Field source)
        {
            if (target is null) return;

            if (source is not null)
            {
                if (target.Boost.HasValue)
                    source.Boost = target.Boost;
                if (target.IsFilterable.HasValue)
                    source.IsFilterable = target.IsFilterable;
                if (target.IncludeInResults.HasValue)
                    source.IncludeInResults = target.IncludeInResults;
                if (!string.IsNullOrWhiteSpace(target.Label))
                    source.Label = target.Label;

                if (target.Fields?.Any() == source.Fields?.Any() == true)
                    target.Fields.Merge(source.Fields);
            }            
        }

        public static Field FromNestProperty(
            this IProperty property,
            string parentFieldName = null)
        {
            var fieldName =
                string.IsNullOrWhiteSpace(parentFieldName) ?
                property.Name.Name :
                string.Join(".", parentFieldName, property.Name.Name).Trim('.');

            return property switch
            {
                ObjectProperty f => BuildField(f, fieldName),
                TextProperty f => BuildField(f, fieldName),
                BooleanProperty f => BuildField(f, fieldName),
                NumberProperty f => BuildField(f, fieldName),
                DateProperty f => BuildField(f, fieldName),
                _ => BuildField(property, fieldName)
            };

        }

        private static Field BuildField(
            IProperty property,
            string fieldName)
        {
            return new Field(
                fieldName,
                property.Type);
        }

        private static Field BuildField(
            ObjectProperty property,
            string fieldName)
        {
            return new Field(
                fieldName,
                (property as IProperty).Type,
                fields: property?.Properties?.Select(x => FromNestProperty(x.Value, fieldName))
            );
        }

        private static Field BuildField(
            TextProperty property,
            string fieldName)
        {
            return new Field(
                fieldName,
                (property as IProperty).Type,
                fields: property?.Fields?.Select(x => FromNestProperty(x.Value, fieldName))
            );
        }

        private static Field BuildField(
            BooleanProperty property,
            string fieldName)
        {
            return new Field(
                fieldName,
                (property as IProperty).Type,
                fields: property?.Fields?.Select(x => FromNestProperty(x.Value, fieldName))
            );
        }

        private static Field BuildField(
            NumberProperty property,
            string fieldName)
        {
            return new Field(
                fieldName,
                (property as IProperty).Type,
                fields: property?.Fields?.Select(x => FromNestProperty(x.Value, fieldName))
            );
        }

        private static Field BuildField(
            DateProperty property,
            string fieldName)
        {
            return new Field(
                fieldName,
                (property as IProperty).Type,
                fields: property?.Fields?.Select(x => FromNestProperty(x.Value, fieldName))
            );
        }
    }

}
