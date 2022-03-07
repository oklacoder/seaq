using System.Threading.Tasks;
using Elasticsearch.Net;

namespace seaq
{

    public class AdvancedQuery :
        ISeaqQuery
    {
        private AdvancedQueryCriteria _criteria;
        public new ISeaqQueryCriteria Criteria => _criteria;

        public AdvancedQuery(
            AdvancedQueryCriteria criteria) 
        {
            _criteria = criteria;
        }

        ISeaqQueryResults ISeaqQuery.Execute(Nest.ElasticClient client)
        {
            return Execute(client);
        }
        public AdvancedQueryResults Execute(Nest.ElasticClient client)
        {
            var results = client.Search<BaseDocument>(_criteria.GetSearchDescriptor());

            return new AdvancedQueryResults(results, _criteria.DeprecatedIndexTargets);
        }
        async Task<ISeaqQueryResults> ISeaqQuery.ExecuteAsync(Nest.ElasticClient client)
        {
            return await ExecuteAsync(client);
        }
        public async Task<AdvancedQueryResults> ExecuteAsync(Nest.ElasticClient client)
        {
            var results = await client.SearchAsync<BaseDocument>(_criteria.GetSearchDescriptor());

            return new AdvancedQueryResults(results, _criteria.DeprecatedIndexTargets);
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

        ISeaqQueryResults<T> ISeaqQuery<T>.Execute(Nest.ElasticClient client)
        {
            return Execute(client);
        }
        public AdvancedQueryResults<T> Execute(Nest.ElasticClient client)
        {
            var query = _criteria.GetSearchDescriptor();

            var results = client.Search<T>(query);

            return new AdvancedQueryResults<T>(results, _criteria.DeprecatedIndexTargets);
        }

        async Task<ISeaqQueryResults<T>> ISeaqQuery<T>.ExecuteAsync(Nest.ElasticClient client)
        {
            return await ExecuteAsync(client);
        }

        public async Task<AdvancedQueryResults<T>> ExecuteAsync(Nest.ElasticClient client)
        {
            var results = await client.SearchAsync<T>(_criteria.GetSearchDescriptor());

            return new AdvancedQueryResults<T>(results, _criteria.DeprecatedIndexTargets);
        }
    }
}
