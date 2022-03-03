using System.Collections.Generic;

namespace seaq
{
    public class IndexConfig
    {
        public string Name { get; set; }
        public string DocumentType { get; set; }
        public string? IndexAsType { get; }
        public IEnumerable<string> Aliases { get; set; }
        public int PrimaryShards { get; set; }
        public int ReplicaShards { get; set; }
        public bool ForceRefreshOnDocumentCommit { get; set; }
        public bool EagerlyPersistSchema { get; set; }
        public bool? IsDeprecated { get; set; }
        public string? DeprecationMessage { get; set; }
        public bool? IsHidden { get; set; }
        public bool? ReturnInGlobalSearch { get; set; }
        public string? ObjectLabel { get; set; }
        public string? ObjectLabelPlural { get; set; }
        public string? PrimaryField { get; set; }
        public string? PrimaryFieldLabel { get; set; }
        public string? SecondaryField { get; set; }
        public string? SecondaryFieldLabel { get; set; }

        public IndexConfig(
            string name,
            string documentType,
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
            Aliases = aliases;
            PrimaryShards = primaryShards ?? Constants.Indices.Defaults.PrimaryShardsDefault;
            ReplicaShards = replicaShards ?? Constants.Indices.Defaults.ReplicaShardsDefault;
            ForceRefreshOnDocumentCommit = forceRefreshOnDocumentCommit ?? Constants.Indices.Defaults.ForceRefreshOnDocumentCommitDefault;
            EagerlyPersistSchema = eagerlyPersistSchema ?? Constants.Indices.Defaults.EagerlyPersistSchema;
            IsDeprecated = isDeprecated;
            DeprecationMessage = deprecationMessage;
            IsHidden = isHidden;
            ReturnInGlobalSearch = returnInGlobalSearch;
            ObjectLabel = objectLabel;
            ObjectLabelPlural = objectLabelPlural;
            PrimaryField = primaryField;
            PrimaryFieldLabel = primaryFieldLabel;
            SecondaryField = secondaryField;
            SecondaryFieldLabel = secondaryFieldLabel;
            IndexAsType = indexAsType;
        }

        public IndexConfig(
            Index index)
        {
            Name = index.Name;
            DocumentType = index.DocumentType;
            Aliases = index.Aliases;
            PrimaryShards = index.PrimaryShards;
            ReplicaShards = index.ReplicaShards;
            ForceRefreshOnDocumentCommit = index.ForceRefreshOnDocumentCommit;
            EagerlyPersistSchema = index.EagerlyPersistSchema;

            IsDeprecated = index.IsDeprecated;
            DeprecationMessage = index.DeprecationMessage;
            IsHidden = index.IsHidden;
            ReturnInGlobalSearch= index.ReturnInGlobalSearch;

            ObjectLabel = index.ObjectLabel;
            ObjectLabelPlural = index.ObjectLabelPlural;
            PrimaryField = index.PrimaryField;
            PrimaryFieldLabel = index.PrimaryFieldLabel;
            SecondaryField = index.SecondaryField;
            SecondaryFieldLabel = index.SecondaryFieldLabel;

        }
    }
}
