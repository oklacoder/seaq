using Seaq.Elasticsearch.Documents;

namespace Seaq.Elasticsearch.Queries
{
    public interface IQuery<TDocument>
        where TDocument : class, IDocument
    {
        ICriteria<TDocument> Criteria { get; }
    }
}
