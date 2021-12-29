using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seaq
{
    public class Constants
    {

        public const string TextPartSeparator = "|||";

        public static class Normalizers
        {
            public const string Lowercase = "lowercase";
        }

        public class Clusters
        {

        }
        public class Indices
        {
            public const string NamePartSeparator = "_";
            public class Meta
            {
                public const string SchemaKey = "seaq|index_schema";
            }
            public class Defaults
            {
                public const int PrimaryShardsDefault = 1;
                public const int ReplicaShardsDefault = 2;
                public const bool ForceRefreshOnDocumentCommitDefault = true;
                public const bool EagerlyPersistSchema = true;
            }
        }
        public class Documents
        {

        }
        public class Fields
        {
            public const string SortField = "sort";
            public const string KeywordField = "keyword";
            public static IEnumerable<string> AlwaysReturnedFields => new string[] { nameof(BaseDocument.Id), nameof(BaseDocument.IndexName), nameof(BaseDocument.Type) };
            public static IEnumerable<string> NestReservedFieldNames => new string[] { "score", "value_as_string", "keys", "max_score", "type" };
            public static IEnumerable<string> NestReservedFieldNameSubs => new string[] { "@score", "@value_as_string", "@keys", "@max_score", "@type" };
        }

    }


}
