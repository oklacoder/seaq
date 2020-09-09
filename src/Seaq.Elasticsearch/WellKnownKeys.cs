using System;
using System.Collections.Generic;
using System.Text;

namespace Seaq.Elasticsearch
{
    public class WellKnownKeys
    {
        public static class IndexSettings
        {
            public const string DotNetType = "dotNetType";
            public const string StoreType = "storeType";
            public const string StoreSchema = "schema";
        }

        public static class Normalizers
        {
            public const string Lowercase = "lowercase";
        }

        public static class Fields
        {
            public const string LowerField = "lower";
            public const string KeywordField = "keyword";
        }

        public static class Queries
        {
            public const string BetweenDelimeter = "|||";
        }
    }
}
