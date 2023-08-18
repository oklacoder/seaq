using Nest;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;

//need to:
//be able to mark deprecated
//return that deprecation warning in a result set

//be able to mark for global return
//ensure global return vs specified index functions

namespace seaq
{
    public class Index : 
        BaseDocument
    {
        /// <summary>
        /// Used for internal index definition store.  The id of this index's document in that store.
        /// </summary>
        public override string Id => Name;
        /// <summary>
        /// Used for internal index definition store.  The index in which this document's definition is saved.
        /// </summary>
        public override string IndexName => GetType().FullName;
        /// <summary>
        /// Used for internal index definition store.  The dotnet type of this index's document in that store.
        /// </summary>
        public override string Type => GetType().FullName;

        /// <summary>
        /// Internal seaq target for all query/indexing operations mapped to this index.  Allows the grouping of implementing types so as to reduce the risk of Elastic oversharding.
        /// </summary>
        public override string IndexAsType { get; set; }

        /// <summary>
        /// The name of the index
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The dotnet type full name of the index's documents
        /// </summary>
        public string DocumentType { get; set; }
        /// <summary>
        /// A list of Elasticsearch aliases applied to this index
        /// </summary>
        public IEnumerable<string> Aliases { get; set; }
        /// <summary>
        /// Number of Elasticsearch primary shards allocated to this index
        /// </summary>
        public int PrimaryShards { get; set; }
        /// <summary>
        /// Number of Elasticsearch replica shards allocated to this index
        /// </summary>
        public int ReplicaShards { get; set; }
        /// <summary>
        /// Whether to force an index refresh after committing documents to this index.  This can cause a performance hit,
        /// the size of which depends on several factors including number of documents committed, but allows the documents
        /// to be immediately searchable, rather than eventually available per Elastic's defaults.
        /// </summary>
        public bool ForceRefreshOnDocumentCommit { get; set; }
        /// <summary>
        /// Save index schema to internal index store automatically
        /// </summary>
        public bool EagerlyPersistSchema { get; set; }
        /// <summary>
        /// List of index fields.  Maps loosely to dotnet type properties.
        /// </summary>
        public IEnumerable<Field> Fields { get; set; } = Array.Empty<Field>();

        /// <summary>
        /// Indicates that the data in this index is deprecated - still maintained, but soon to be unsupported
        /// </summary>
        public bool IsDeprecated { get; set; }

        /// <summary>
        /// The index that contains the newest version of this data, and should be referenced instead
        /// </summary>
        public string DeprecationMessage { get; set; }

        /// <summary>
        /// Functions as a soft delete.  Allows to retain the index temporarily, but eliminates it from search results.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Includes this index for consideration in queries where no type or index list is specified
        /// </summary>
        public bool ReturnInGlobalSearch { get; set; }

        /// <summary>
        /// Display value for an object from this index.  Eg an index with DocumentType seaq.Examples.Model.TestDocument could have a ObjectLabel value of "TestDocument"
        /// </summary>
        public string ObjectLabel { get; set; }
        /// <summary>
        /// Display value for a collection of objects from this index.  Eg an index with DocumentType seaq.Examples.Model.TestDocument could have a ObjectLabelPlural value of "TestDocuments"
        /// </summary>
        public string ObjectLabelPlural { get; set; }
        /// <summary>
        /// Primary display value for objects from this index.  Typically a human-readable identifier - not a surrogate key
        /// </summary>
        public string PrimaryField { get; set; }
        /// <summary>
        /// A display-friendly name for the PrimaryField, which will be camelCased and potentially contain 1:n dots/name pieces
        /// </summary>
        public string PrimaryFieldLabel { get; set; }
        /// <summary>
        /// Secondary display value for objects from this index.  Typically a human-readable identifier - not a surrogate key
        /// </summary>
        public string SecondaryField { get; set; }
        /// <summary>
        /// A display-friendly name for the SecondaryField, which will be camelCased and potentially contain 1:n dots/name pieces
        /// </summary>
        public string SecondaryFieldLabel { get; set; }
        /// <summary>
        /// A collection of other, implementation-driven index details that can't be cleanly mapped onto the provided fields.  These aren't used by seaq directly in any way.  
        /// Map objects will be converted into form Dictionary<string, object>.
        /// </summary>
        public Dictionary<string, object> Meta { get; set; }

        private Index(
            string name,
            string documentType,
            IEnumerable<Field> fields,
            IEnumerable<string> aliases = null,
            int? primaryShards = null,
            int? replicaShards = null,
            bool? forceRefreshOnDocumentCommit = null,
            bool? eagerlyPersistSchema = null,
            bool? isDeprecated = null,
            string deprecationMessage = null,
            bool? isHidden = null,
            bool? returnInGlobalSearch = null,
            string objectLabel = null,
            string objectLabelPlural = null,
            string primaryField = null,
            string primaryFieldLabel = null,
            string secondaryField = null,
            string secondaryFieldLabel = null,
            string indexAsType = null)
        {
            Name = name;
            DocumentType = documentType;
            IndexAsType = indexAsType;
            Fields = fields;
            Aliases = aliases;
            PrimaryShards = primaryShards ?? Constants.Indices.Defaults.PrimaryShardsDefault;
            ReplicaShards = replicaShards ?? Constants.Indices.Defaults.ReplicaShardsDefault;
            ForceRefreshOnDocumentCommit = forceRefreshOnDocumentCommit ?? Constants.Indices.Defaults.ForceRefreshOnDocumentCommitDefault;
            EagerlyPersistSchema = eagerlyPersistSchema ?? Constants.Indices.Defaults.EagerlyPersistSchema;

            IsDeprecated = isDeprecated ?? Constants.Indices.Defaults.IsDeprecated;
            DeprecationMessage = deprecationMessage ?? Constants.Indices.Defaults.DeprecationMessage;
            IsHidden = isHidden ?? Constants.Indices.Defaults.IsHidden;
            ReturnInGlobalSearch = returnInGlobalSearch ?? Constants.Indices.Defaults.ReturnInGlobalSearch;
            ObjectLabel = objectLabel ?? Constants.Indices.Defaults.ObjectLabel;
            ObjectLabelPlural = objectLabelPlural ?? Constants.Indices.Defaults.ObjectLabelPlural;
            PrimaryField = primaryField ?? Constants.Indices.Defaults.PrimaryField;
            PrimaryFieldLabel = primaryFieldLabel ?? Constants.Indices.Defaults.PrimaryFieldLabel;
            SecondaryField = secondaryField ?? Constants.Indices.Defaults.SecondaryField;
            SecondaryFieldLabel = secondaryFieldLabel ?? Constants.Indices.Defaults.SecondaryFieldLabel;
        }

        public Index()
        {

        }

        public Index(
            IndexConfig config)
        {
            Name = config.Name;
            DocumentType = config.DocumentType;
            IndexAsType = config.IndexAsType;
            Aliases = config.Aliases;
            PrimaryShards = config.PrimaryShards;
            ReplicaShards = config.ReplicaShards;
            ForceRefreshOnDocumentCommit = config.ForceRefreshOnDocumentCommit;
            EagerlyPersistSchema = config.EagerlyPersistSchema;

            IsDeprecated = config.IsDeprecated ?? Constants.Indices.Defaults.IsDeprecated;
            DeprecationMessage = config.DeprecationMessage ?? Constants.Indices.Defaults.DeprecationMessage;
            IsHidden = config.IsHidden ?? Constants.Indices.Defaults.IsHidden;
            ReturnInGlobalSearch = config.ReturnInGlobalSearch ?? Constants.Indices.Defaults.ReturnInGlobalSearch;
            ObjectLabel = config.ObjectLabel ?? Constants.Indices.Defaults.ObjectLabel;
            ObjectLabelPlural = config.ObjectLabelPlural ?? Constants.Indices.Defaults.ObjectLabelPlural;
            PrimaryField = config.PrimaryField ?? Constants.Indices.Defaults.PrimaryField;
            PrimaryFieldLabel = config.PrimaryFieldLabel ?? Constants.Indices.Defaults.PrimaryFieldLabel;
            SecondaryField = config.SecondaryField ?? Constants.Indices.Defaults.SecondaryField;
            SecondaryFieldLabel = config.SecondaryFieldLabel ?? Constants.Indices.Defaults.SecondaryFieldLabel;
        }

        public static Index Create(
            KeyValuePair<IndexName, IndexState> index)
        {
            var name = index.Key.Name;
            Log.Verbose("Bulding index definition for {0}", name);

            Index resp;
            var schema = index.Value?.Mappings?.Meta?.ContainsKey(Constants.Indices.Meta.SchemaKey) == true 
                ? index.Value.Mappings.Meta[Constants.Indices.Meta.SchemaKey] 
                : null;


            if (schema is not null)
            {
                resp = System.Text.Json.JsonSerializer.Deserialize<Index>(
                    System.Text.Json.JsonSerializer.Serialize(schema), 
                    new System.Text.Json.JsonSerializerOptions() { 
                        PropertyNameCaseInsensitive = true
                    });
                if (resp.Meta != null)
                {
                    resp.Meta = resp.Meta
                        .Select(kvp =>
                        {
                            if (kvp.Value.GetType() == typeof(System.Text.Json.JsonElement))
                            {
                                var v = GetMetaValueAsOriginalType((System.Text.Json.JsonElement)kvp.Value);
                                return new KeyValuePair<string, object>(kvp.Key, v);
                            }
                            else
                            {
                                return kvp;
                            }
                        })
                        .ToDictionary(x => x.Key, x => x.Value);
                }
            }
            else
            {
                resp = new Index(
                    name,
                    nameof(BaseDocument),
                    index.Value?.Mappings?.Properties?
                        .Select(x => x.Value?.FromNestProperty()));
            }

            if (resp.Aliases?.Any() is not true)
            {
                resp.Aliases = index.Value.Aliases.Select(x => x.Key.Name);
            }

            if (string.IsNullOrWhiteSpace(resp.IndexAsType) is not true)
                return resp;
            
            var fieldList = index.Value?.Mappings?.Properties?.Select(x => x.Value.FromNestProperty());
            
            if (resp.Fields?.Any() is not true)
            {
                resp.Fields = fieldList;
            }
            else if (!fieldList.All(x => resp.Fields.Any(z => x.Name.Equals(z.Name, StringComparison.OrdinalIgnoreCase))))
            {
                resp.Fields = resp.Fields.Merge(fieldList);
            }

            return resp;
        }

        public static object GetMetaValueAsOriginalType(System.Text.Json.JsonElement el)
        {
            return el.ValueKind switch
            {
                System.Text.Json.JsonValueKind.String => GetJsonStringAsDotnetType(el),
                System.Text.Json.JsonValueKind.Number => GetJsonNumberAsDotnetType(el),
                System.Text.Json.JsonValueKind.Object => GetJsonObjectAsDotnet(el),
                System.Text.Json.JsonValueKind.Array => GetJsonArrayAsDotnet(el),
                System.Text.Json.JsonValueKind.False => false,
                System.Text.Json.JsonValueKind.True => true,
                _ => el
            };
        }
        public static object GetJsonStringAsDotnetType(
            System.Text.Json.JsonElement el)
        {
            var s = el.ToString();
            if (DateTime.TryParse(s, out var dt))
                return dt;
            else return s;
        }
        public static object GetJsonNumberAsDotnetType(
            System.Text.Json.JsonElement el)
        {
            if (el.TryGetInt16(out var sh))
                return sh;
            if (el.TryGetInt32(out var i))
                return i;
            if (el.TryGetInt64(out var ln))
                return ln;
            else if (el.TryGetDouble(out var db))
                return db;
            else return el;
        }
        public static IEnumerable<object> GetJsonArrayAsDotnet(
            System.Text.Json.JsonElement el)
        {
            return el.EnumerateArray().Select(x => GetMetaValueAsOriginalType(x));
        }
        public static object GetJsonObjectAsDotnet(
            System.Text.Json.JsonElement el)
        {
            var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(el);
            foreach(var k in dict.Keys)
            {
                if (dict[k].GetType() == typeof(System.Text.Json.JsonElement))
                {
                    var v = (System.Text.Json.JsonElement)dict[k];
                    dict[k] = GetMetaValueAsOriginalType(v);
                }
            }
            return dict;
        }
    }
}
