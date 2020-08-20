using Elasticsearch.Net;
using System;

namespace Seaq.Elasticsearch.Clusters
{
    public interface ISeaqElasticsearchSerializer :
        IElasticsearchSerializer
    {
        T Deserialize<T>(object data);
    }
}