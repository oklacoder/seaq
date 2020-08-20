using Nest;

namespace Seaq.Elasticsearch.Queries
{
    public interface IQueryBuilder
    {
        SourceFilterDescriptor<T> BuildSourceFilter<T>(string[] fields) where T : class;
        QueryContainerDescriptor<T> GetMatchPhrasePrefixQueryForFilter<T>(QueryFilter filter) where T : class;
        QueryContainerDescriptor<T> GetMatchPhraseQueryForFilter<T>(QueryFilter filter) where T : class;
        QueryContainerDescriptor<T> GetMatchQueryForFilter<T>(QueryFilter filter) where T : class;
    }
}