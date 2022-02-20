using Elastic.Clients.Elasticsearch;

namespace seaq
{
    public abstract class SeaqElasticsearchSerializer :
        SourceSerializer
    {
        protected abstract T Deserialize<T>(object data);
    }
}
