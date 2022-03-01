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
        public static class Tokenizers
        {
            public const string Letter = "letter";
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
                public const bool IsDeprecated = false;
                public const string DeprecationMessage = null;
                public const bool IsHidden = false;
                public const bool ReturnInGlobalSearch = false;
                public const string ObjectLabel = null;
                public const string ObjectLabelPlural = null;
                public const string PrimaryField = null;
                public const string PrimaryFieldLabel = null;
                public const string SecondaryField = null;
                public const string SecondaryFieldLabel = null;
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
            public static IEnumerable<string> NestReservedFieldNames => new string[] { 
                "after_key",
                "_as_string",
                "bg_count",
                "bottom_right",
                "bounds",
                "buckets",
                "count",
                "doc_count",
                "doc_count_error_upper_bound",
                "fields",
                "from",
                "top",
                "type",
                "from_as_string",
                "hits",
                "key",
                "key_as_string",
                "keys",
                "location",
                "max_score",
                "meta",
                "min",
                "min_length",
                "score",
                "sum_other_doc_count",
                "to",
                "to_as_string",
                "top_left",
                "total",
                "value",
                "value_as_string",
                "values",
                "geometry",
                "properties "
            };
            public static IEnumerable<string> NestReservedFieldNameSubs => new string[] 
            {
                "@after_key",
                "@_as_string",
                "@bg_count",
                "@bottom_right",
                "@bounds",
                "@buckets",
                "@count",
                "@doc_count",
                "@doc_count_error_upper_bound",
                "@fields",
                "@from",
                "@top",
                "@type",
                "@from_as_string",
                "@hits",
                "@key",
                "@key_as_string",
                "@keys",
                "@location",
                "@max_score",
                "@meta",
                "@min",
                "@min_length",
                "@score",
                "@sum_other_doc_count",
                "@to",
                "@to_as_string",
                "@top_left",
                "@total",
                "@value",
                "@value_as_string",
                "@values",
                "@geometry",
                "@properties "
            };
        }

    }


}
