namespace Seaq.Clusters
{
    public interface ISqeeElasticsearchSerializer :
        Elasticsearch.Net.IElasticsearchSerializer
    {
        T Deserialize<T>(object data);
    }

}
