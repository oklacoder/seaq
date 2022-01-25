using Nest;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private Index(
            string name,
            string documentType,
            IEnumerable<Field> fields,
            IEnumerable<string> aliases = null,
            int? primaryShards = null,
            int? replicaShards = null,
            bool? forceRefreshOnDocumentCommit = null,
            bool? eagerlyPersistSchema = null)
        {
            Name = name;
            DocumentType = documentType;
            Fields = fields;
            Aliases = aliases;
            PrimaryShards = primaryShards ?? Constants.Indices.Defaults.PrimaryShardsDefault;
            ReplicaShards = replicaShards ?? Constants.Indices.Defaults.ReplicaShardsDefault;
            ForceRefreshOnDocumentCommit = forceRefreshOnDocumentCommit ?? Constants.Indices.Defaults.ForceRefreshOnDocumentCommitDefault;
            EagerlyPersistSchema = eagerlyPersistSchema ?? Constants.Indices.Defaults.EagerlyPersistSchema;
        }

        public Index()
        {

        }

        public Index(
            IndexConfig config)
        {
            Name = config.Name;
            DocumentType = config.DocumentType;
            Aliases = config.Aliases;
            PrimaryShards = config.PrimaryShards;
            ReplicaShards = config.ReplicaShards;
            ForceRefreshOnDocumentCommit = config.ForceRefreshOnDocumentCommit;
            EagerlyPersistSchema = config.EagerlyPersistSchema;
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
            
            var fieldList = index.Value?.Mappings?.Properties?.Select(x => x.Value.FromNestProperty());
            
            if (resp.Fields?.Any() is not true)
            {
                resp.Fields = fieldList;
            }
            else if (!fieldList.All(x => resp.Fields.Any(z => x.Name.Equals(z.Name, StringComparison.OrdinalIgnoreCase))))
            {
                resp.Fields.Merge(fieldList);
            }

            return resp;
        }
    }
}
