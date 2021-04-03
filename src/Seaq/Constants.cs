using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seaq
{
    public static class Constants
    {
        public const string TextPartSeparator = "|||";

        public static class Normalizers
        {
            public const string Lowercase = "lowercase";
        }

        public static class CollectionSettings
        {
            public const string SchemaKey = "collectionSchema";
            public const string CollectionNamePartSeparator = "_";
        }

        public static class Fields
        {
            public const string SortField = "sort";
            public const string KeywordField = "keyword";
            public static IEnumerable<string> AlwaysReturnedFields => new string[] { nameof(IDocument.Id), nameof(IDocument.CollectionId), nameof(IDocument.Type) };
        }

    }
}
