using Nest;
using Newtonsoft.Json;
using Seaq.Elasticsearch.Documents;
using Seaq.Elasticsearch.Extensions;
using Seaq.Elasticsearch.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Seaq.Elasticsearch.Stores
{
    [DataContract]
    public class StoreSchema
    {
        [DataMember(Name = nameof(StoreId))]
        public string StoreId { get; }

        [DataMember(Name = nameof(Type))]
        public string Type => this.GetType().FullName;

        [DataMember(Name = nameof(StoreType))]
        public string StoreType { get; }

        [DataMember(Name = nameof(Fields))]
        public StoreField[] Fields { get; }


        [JsonConstructor]
        public StoreSchema(
            string storeId,
            string type,
            string storeType,
            StoreField[] fields)
        {
            StoreId = storeId;
            StoreType = storeType;
            Fields = fields;
        }
        public StoreSchema(
            string storeId,
            string storeType,
            StoreField[] fields)
        {
            StoreId = storeId;
            StoreType = storeType;
            Fields = fields;
        }

        public StoreSchema(
            KeyValuePair<IndexName, IndexState> index)
        {
            var properties = index.Value?.Mappings?.Properties;

            var keys = properties.Keys;

            var propertyDictionary = new Dictionary<string, StoreField>();

            foreach (var key in keys)
            {
                propertyDictionary.Add(key.Name, properties[key].ToStoreField());
            }


            StoreId = index.Key.Name;
            
            if (index.Value?.Mappings?.Meta?.ContainsKey(WellKnownKeys.IndexSettings.StoreSchema) == true)
            {
                var typedMetaSchema = Newtonsoft.Json.JsonConvert.DeserializeObject<StoreSchema>(
                    JsonConvert.SerializeObject(index.Value?.Mappings?.Meta?[WellKnownKeys.IndexSettings.StoreSchema]));
                StoreType = typedMetaSchema.StoreType;
            }
            Fields = propertyDictionary.Values.ToArray();
            
        }

        public StoreSchema(
            CreateStoreSettings settings)
        {
            StoreId = Stores.StoreId.FormatAsIndexId(settings.ScopeId, settings.Moniker);
            StoreType = settings.Type.FullName;
        }

        public string[] GetAggregatableFieldNames()
        {
            return Fields?
                .Where(x => x.IsFilterable == true)?
                .SelectMany(x => x?.GetKeywordFieldNames)?
                .ToArray() ??
                new string[] { };
        }

        public string[] GetAggregatableFieldNames(IFieldNameUtilities fieldNameUtilities, params string[] fieldNames)
        {
            var type = fieldNameUtilities.GetSearchableType(this.StoreType);
            var fns = fieldNames.Select(x => fieldNameUtilities.GetElasticPropertyNameWithoutSuffix(type, x));
            return Fields?
                .Where(x => x?.IsFilterable == true && 
                    //fns.Any(fn => fn.Equals(x?.Name, StringComparison.OrdinalIgnoreCase))
                    fns.Any(fn => x?.FieldTree?.Any(z => z.Equals(fn, StringComparison.OrdinalIgnoreCase)) == true)
                )
                .SelectMany(x => x.GetKeywordFieldNames)
                .ToArray() ??
                new string[] { };
        }

        public string[] GetSortableFieldNames()
        {
            return Fields?
                .Where(x => x.IsFilterable == true)?
                .SelectMany(x => x?.GetSortFieldNames)?
                .ToArray() ??
                new string[] { };
        }

        public string[] GetSortableFieldNames(IFieldNameUtilities fieldNameUtilities, params string[] fieldNames)
        {
            var type = fieldNameUtilities.GetSearchableType(this.StoreType);
            var fns = fieldNames.Select(x => fieldNameUtilities.GetElasticPropertyNameWithoutSuffix(type, x));
            return Fields?
                .Where(x => x?.IsFilterable == true &&
                    //fns.Any(fn => fn.Equals(x?.Name, StringComparison.OrdinalIgnoreCase))
                    fns.Any(fn => x?.FieldTree?.Any(z => z.Equals(fn, StringComparison.OrdinalIgnoreCase)) == true)
                )
                .SelectMany(x => x.GetSortFieldNames)
                .ToArray() ??
                new string[] { };
        }



        public string[] GetAllBoostedFieldNames()
        {
            return Fields?
                .Select(x => x.GetBoostedFieldName)
                .ToArray() ??
                new string[] { };
        }

        public string[] GetBoostedTextFieldNames()
        {
            return Fields?
                .Where(x => x.Type == "text")
                .Select(x => x.GetBoostedFieldName)
                .ToArray() ??
                new string[] { };
        }
    }
}
