using Serilog;
using System;

namespace seaq
{
    public static class IndexNameUtilities
    {
        public static string FormatIndexName(
            this Type type,
            string clusterScope)
        {
            return type?.FullName.FormatIndexName(clusterScope);
        }
        public static string FormatIndexName(
            this string indexName,
            string clusterScope)
        {
            return indexName?
                .CoerceIndexNameStartsWithScope(clusterScope)?
                .CoerceIndexNameCasing();
        }

        private static string CoerceIndexNameCasing(
            this string indexName)
        {

            if (!indexName.Equals(indexName.ToLowerInvariant()))
            {
                var oldName = indexName;
                indexName = indexName.ToLowerInvariant();

                Log.Information("Provided index name ({0}) was not all lowercase as required - index name coerced to {1}", oldName, indexName);
            }
            return indexName;
        }
        private static string CoerceIndexNameStartsWithScope(
            this string indexName,
            string clusterScope)
        {
            if (!indexName.StartsWith(clusterScope, StringComparison.OrdinalIgnoreCase))
            {
                var oldName = indexName;
                indexName = string.Join(Constants.Indices.NamePartSeparator, clusterScope, indexName);

                Log.Information("Provided index name ({0}) did not begin with cluster's scope as required - index name coerced to {1}", oldName, indexName);
            }
            return indexName;
        }
    }
}
