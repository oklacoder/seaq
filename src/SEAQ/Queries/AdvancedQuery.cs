using System.Threading.Tasks;
using Elasticsearch.Net;

namespace seaq
{

    public class AdvancedQuery :
        AdvancedQuery<BaseDocument>
    {
        private AdvancedQueryCriteria<BaseDocument> _criteria;
        public new ISeaqQueryCriteria<BaseDocument> Criteria => _criteria;

        public AdvancedQuery(
            AdvancedQueryCriteria criteria) 
            : base(criteria)
        {
            _criteria = criteria;
        }

        public new ISeaqQueryResults<BaseDocument> Execute(Nest.ElasticClient client)
        {
            var results = client.Search<BaseDocument>(_criteria.GetSearchDescriptor());

            return new AdvancedQueryResults<BaseDocument>(results);
        }

        public new async Task<ISeaqQueryResults<BaseDocument>> ExecuteAsync(Nest.ElasticClient client)
        {
            var results = await client.SearchAsync<BaseDocument>(_criteria.GetSearchDescriptor());

            return new AdvancedQueryResults<BaseDocument>(results);
        }
    }
    public class AdvancedQuery<T> :
        ISeaqQuery<T>
    where T : BaseDocument
    {
        private AdvancedQueryCriteria<T> _criteria;
        public virtual ISeaqQueryCriteria<T> Criteria => _criteria;

        public AdvancedQuery(
            AdvancedQueryCriteria<T> criteria)
        {
            _criteria = criteria;
        }

        public ISeaqQueryResults<T> Execute(Nest.ElasticClient client)
        {
            var query = _criteria.GetSearchDescriptor();

            var json = client.RequestResponseSerializer.SerializeToString(query, SerializationFormatting.Indented);

            var results = client.Search<T>(query);

            return new AdvancedQueryResults<T>(results);
        }

        public async Task<ISeaqQueryResults<T>> ExecuteAsync(Nest.ElasticClient client)
        {
            var results = await client.SearchAsync<T>(_criteria.GetSearchDescriptor());

            return new AdvancedQueryResults<T>(results);
        }
    }
}
