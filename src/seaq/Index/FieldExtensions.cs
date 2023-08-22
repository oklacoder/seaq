using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public static class FieldExtensions
    {
        public static IEnumerable<Field> Merge(
            this IEnumerable<Field> target,
            IEnumerable<Field> source)
        {
            if (target is null) return Array.Empty<Field>();

            var resp = new List<Field>();
            resp.AddRange(target);

            foreach (var src in source)
            {
                var tgt = resp.FirstOrDefault(x => x.Name.Equals(src.Name));
                if (tgt is null)
                    resp.Add(src);
            }

            return resp;
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
