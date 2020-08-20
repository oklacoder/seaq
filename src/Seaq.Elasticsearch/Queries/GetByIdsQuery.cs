using Seaq.Elasticsearch.Documents;

namespace Seaq.Elasticsearch.Queries
{
    public class GetByIdsQuery :
        IQuery<IDocument>
    {
        public GetByIdsQuery(
            GetByIdsQueryCriteria criteria)
        {
            Criteria = criteria;
        }

        public ICriteria<IDocument> Criteria { get; }
    }
}
