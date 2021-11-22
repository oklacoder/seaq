using System;

namespace seaq
{
    public static class IndexNameUtilities
    {
        public static string FormatIndexName(
            Type type)
        {
            return FormatIndexName(type?.FullName);
        }
        public static string FormatIndexName(
            string indexName)
        {
            return indexName?.ToLowerInvariant();
        }
    }
}
