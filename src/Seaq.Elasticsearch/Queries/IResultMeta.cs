namespace Seaq.Elasticsearch.Queries
{
    public interface IResultMeta
    {
        string[] ResultDocumentStores { get; }
        long Took { get; }
    }
}