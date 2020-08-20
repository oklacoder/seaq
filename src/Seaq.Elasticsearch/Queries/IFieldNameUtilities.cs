using System;
using System.Collections.Generic;

namespace Seaq.Elasticsearch.Queries
{
    public interface IFieldNameUtilities
    {
        IEnumerable<Type> GetAllSearchableTypes();
        string GetElasticPropertyName(
            Type type,
            string propertyName);

        string GetElasticPropertyNameWithoutSuffix(
            Type type,
            string propertyName);

        public Type GetSearchableType(
            string typeFullName);
    }
}
