namespace seaq
{
    public interface ISeaqElasticsearchSerializer :
        Elasticsearch.Net.IElasticsearchSerializer
    {
        T Deserialize<T>(object data);
    }
}
