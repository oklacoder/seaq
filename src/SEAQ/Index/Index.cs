using Nest;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class Index
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int PrimaryShards { get; set; }
        public int ReplicaShards { get; set; }
        public bool ForceRefreshOnDocumentCommit { get; set; }
        public bool EagerlyPersistSchema { get; set; }
        public IEnumerable<Field> Fields { get; set; }


        private Index(
            string name,
            string type,
            IEnumerable<Field> fields,
            int? primaryShards = null,
            int? replicaShards = null,
            bool? forceRefreshOnDocumentCommit = null,
            bool? eagerlyPersistSchema = null)
        {
            Name = name;
            Type = type;
            Fields = fields;
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
            Type = config.Type;
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

            var schema = index.Value?.Mappings?.Meta?.ContainsKey(Constants.Indices.Meta.SchemaKey) == true 
                ? index.Value.Mappings.Meta[Constants.Indices.Meta.SchemaKey] 
                : null;


            if (schema is not null)
            {
                return System.Text.Json.JsonSerializer.Deserialize<Index>(
                    System.Text.Json.JsonSerializer.Serialize(schema), 
                    new System.Text.Json.JsonSerializerOptions() { 
                        PropertyNameCaseInsensitive = true
                    });
            }
            else
            {
                return new Index(
                    name, 
                    nameof(IDocument),
                    index.Value?.Mappings?.Properties
                        .Select(x => x.Value.FromNestProperty()));
            }
        }
    }
}
