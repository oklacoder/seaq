using Nest;
using Seaq.Elasticsearch.Documents;

namespace Seaq.Elasticsearch.Queries
{
    public interface IResultsBuilder
    {
        IQueryResult GetQueryResultByCriteriaType<TDocument>(ICriteria<TDocument> criteria, ISearchResponse<IDocument> searchResults) where TDocument : class, IDocument;
    }
}