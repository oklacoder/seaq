using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Seaq.Clusters{
    public class CollectionSchema :
        ICollectionSchema
    {
        public string CollectionName { get; set; }

        public string CollectionDocumentType { get; set; }

        private List<CollectionField> _fields { get; set; } = new List<CollectionField>();

        public IEnumerable<ICollectionField> Fields => _fields;

        public CollectionSchema()
        {

        }

        public CollectionSchema(
            ICollectionConfig config)
        {
            CollectionName = config.Name;
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

            ICollectionField GetChildFieldByName(string fieldName, ICollectionField field)
            {
                if (field.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                    return field;

                var f = field.Fields.FirstOrDefault(x => x.FieldTree.Any(z => z.Equals(fieldName, StringComparison.OrdinalIgnoreCase)));

                return GetChildFieldByName(fieldName, f);
            }
        }

        public void AddField(CollectionField field)
        {
            if (_fields.Any(x => x.Name.Equals(field.Name, StringComparison.OrdinalIgnoreCase)))
            {
                Log.Error("Cannot add field {0} to schema for collection {1} - it already exists.", field.Name, this.CollectionName);
                throw new Exception($"Cannot add field {field.Name} to schema for collection {this.CollectionName} - it already exists.");
            }
            this._fields.Add(field);
        }
    }

}
