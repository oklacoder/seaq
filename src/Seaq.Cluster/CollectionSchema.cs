using Nest;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Seaq.Clusters{
    public class CollectionSchema 
    {
        public string CollectionName { get; set; }

        public string CollectionDocumentType { get; set; }

        public List<CollectionField> Fields { get; set; } = new List<CollectionField>();

        public CollectionSchema()
        {

        }

        public CollectionSchema(
            string collectionName,
            string collectionDocumentType,
            IEnumerable<CollectionField> fields = null)
        {
            CollectionName = collectionName;
            CollectionDocumentType = collectionDocumentType;
            Fields = fields?.ToList() ?? new List<CollectionField>();
        }

        public CollectionSchema(
            CollectionConfig config)
        {
            CollectionName = config.Name;
            CollectionDocumentType = config.DocumentType.FullName;
        }

        public CollectionSchema(
             KeyValuePair<IndexName, IndexState> index,
             CollectionConfig config)
        {
            var properties = index.Value?.Mappings?.Properties;

            foreach(var key in properties.Keys)
            {
                Fields.Add(properties[key].ToCollectionField());
            }

            CollectionName = index.Key.Name;
            CollectionDocumentType = config.DocumentType.FullName;
        }

        public IEnumerable<string> GetFilterableFieldNames(
            params string[] fieldNames)
        {
            var type = FieldNameUtilities.GetSearchableType(CollectionDocumentType);
            var fns = fieldNames.Select(x => FieldNameUtilities.GetElasticPropertyNameWithoutSuffix(type, x));
            return Fields?
                .Where(x => x?.IsFilterable == true &&
                    fns.Any(fns => x?.FieldTree?.Any(y => y.Equals(fns, StringComparison.OrdinalIgnoreCase)) == true))
                .SelectMany(x => x.AllKeywordFields) 
                ?? Array.Empty<string>();
        }

        public IEnumerable<string> GetBoostedFieldNames(params string[] fieldNames)
        {
            var type = FieldNameUtilities.GetSearchableType(CollectionDocumentType);
            var fns = fieldNames.Select(x => FieldNameUtilities.GetElasticPropertyNameWithoutSuffix(type, x));

            return Fields?
                .Where(x => x?.HasBoostedField == true &&
                    fns.Any(fns => x?.FieldTree?.Any(y => y.Equals(fns, StringComparison.OrdinalIgnoreCase)) == true))
                .SelectMany(x => x.AllBoostedFields);
        }

        public IEnumerable<string> GetIncludedFieldNames(params string[] fieldNames)
        {
            var type = FieldNameUtilities.GetSearchableType(CollectionDocumentType);
            var fns = fieldNames.Select(x => FieldNameUtilities.GetElasticPropertyNameWithoutSuffix(type, x));

            return Fields?
                .Where(x => x?.HasIncludedField == true &&
                    fns.Any(fns => x?.FieldTree?.Any(y => y.Equals(fns, StringComparison.OrdinalIgnoreCase)) == true))
                .SelectMany(x => x.AllIncludedFields);
        }

        public IEnumerable<string> GetKeywordFieldNames(params string[] fieldNames)
        {
            var type = FieldNameUtilities.GetSearchableType(CollectionDocumentType);
            var fns = fieldNames.Select(x => FieldNameUtilities.GetElasticPropertyNameWithoutSuffix(type, x));

            return Fields?
                .Where(x => x?.HasKeywordField == true &&
                    fns.Any(fns => x?.FieldTree?.Any(y => y.Equals(fns, StringComparison.OrdinalIgnoreCase)) == true))
                .SelectMany(x => x.AllKeywordFields);
        }

        public IEnumerable<string> GetSortFieldNames(params string[] fieldNames)
        {
            var type = FieldNameUtilities.GetSearchableType(CollectionDocumentType);
            var fns = fieldNames.Select(x => FieldNameUtilities.GetElasticPropertyNameWithoutSuffix(type, x));

            return Fields?
                .Where(x => x?.HasSortField == true &&
                    fns.Any(fns => x?.FieldTree?.Any(y => y.Equals(fns, StringComparison.OrdinalIgnoreCase)) == true))
                .SelectMany(x => x.AllSortFields);
        }

        public CollectionField GetNDepthFieldByName(string fieldName)
        {
            var field = Fields.FirstOrDefault(x => x.FieldTree.Any(z => z.Equals(fieldName, StringComparison.OrdinalIgnoreCase)));

            if (field == null)
                return default;

            return GetChildFieldByName(fieldName, field) as CollectionField;

            CollectionField GetChildFieldByName(string fieldName, CollectionField field)
            {
                if (field.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                    return field;

                var f = field.Fields.FirstOrDefault(x => x.FieldTree.Any(z => z.Equals(fieldName, StringComparison.OrdinalIgnoreCase)));

                return GetChildFieldByName(fieldName, f);
            }
        }

        public void AddField(CollectionField field)
        {
            if (Fields.Any(x => x.Name.Equals(field.Name, StringComparison.OrdinalIgnoreCase)))
            {
                Log.Error("Cannot add field {0} to schema for collection {1} - it already exists.", field.Name, this.CollectionName);
                throw new Exception($"Cannot add field {field.Name} to schema for collection {this.CollectionName} - it already exists.");
            }
            this.Fields.Add(field);
        }
    }

}
