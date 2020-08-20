namespace Seaq.Elasticsearch.Queries
{
    public interface IBucketValue
    {
        string Key { get; }
        int Count { get; }
    }
}