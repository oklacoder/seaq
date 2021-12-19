using Nest;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class Index : 
        BaseDocument
    {
        public override string Id => Name;
        public override string IndexName => GetType().FullName;
        public override string Type => GetType().FullName;

        public string Name { get; set; }
        public string DocumentType { get; set; }
        public IEnumerable<string> Aliases { get; set; }
        public int PrimaryShards { get; set; }
        public int ReplicaShards { get; set; }
        public bool ForceRefreshOnDocumentCommit { get; set; }
        public bool EagerlyPersistSchema { get; set; }
        public IEnumerable<Field> Fields { get; set; } = Array.Empty<Field>();


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
