using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

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

        ISeaqQueryResults ISeaqQuery.Execute(ElasticsearchClient client)
        {
            return Execute(client);
        }
        public AdvancedQueryResults Execute(ElasticsearchClient client)
        {
            var results = client.Search<BaseDocument>(x => _criteria.GetSearchDescriptor());

            return new AdvancedQueryResults(results, _criteria.DeprecatedIndexTargets);
        }
        async Task<ISeaqQueryResults> ISeaqQuery.ExecuteAsync(ElasticsearchClient client)
        {
            return await ExecuteAsync(client);
        }
        public async Task<AdvancedQueryResults> ExecuteAsync(ElasticsearchClient client)
        {
            var results = await client.SearchAsync<BaseDocument>(x => _criteria.GetSearchDescriptor());

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

        ISeaqQueryResults<T> ISeaqQuery<T>.Execute(ElasticsearchClient client)
        {
            return Execute(client);
        }
        public AdvancedQueryResults<T> Execute(ElasticsearchClient client)
        {
            var results = client.Search<T>(x => _criteria.GetSearchDescriptor());

            return new AdvancedQueryResults<T>(results, _criteria.DeprecatedIndexTargets);
        }

        async Task<ISeaqQueryResults<T>> ISeaqQuery<T>.ExecuteAsync(ElasticsearchClient client)
        {
            return await ExecuteAsync(client);
        }

        public async Task<AdvancedQueryResults<T>> ExecuteAsync(ElasticsearchClient client)
        {
            var results = await client.SearchAsync<T>(x => _criteria.GetSearchDescriptor());

            return new AdvancedQueryResults<T>(results, _criteria.DeprecatedIndexTargets);
        }
    }
}
