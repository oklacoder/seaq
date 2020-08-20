using Seaq.Elasticsearch.Documents;

namespace Seaq.Elasticsearch.Queries
{
    public class SuggestionQuery :
        IQuery<ISkinnyDocument>
    {
        private SuggestionQueryCriteria _criteria { get; }
        public ICriteria<ISkinnyDocument> Criteria => _criteria;


        public SuggestionQuery(
            SuggestionQueryCriteria criteria)
        {
            _criteria = criteria;
        }
    }
}