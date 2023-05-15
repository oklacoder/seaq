using Nest;
using System;
using System.Threading.Tasks;

namespace seaq
{
    public class AggregationQuery :
        ISeaqQuery
    {
        private AggregationQueryCriteria _criteria;
        public ISeaqQueryCriteria Criteria => _criteria;

        public AggregationQuery(
            AggregationQueryCriteria criteria)
        {
            _criteria = criteria;
        }

        public ISeaqQueryResults Execute(
            ElasticClient client)
        {

            var query = _criteria.GetSearchDescriptor();

            var results = client.Search<BaseDocument>(query);

            return new AggregationQueryResults(results, _criteria);
        }

        public async Task<ISeaqQueryResults> ExecuteAsync(
            ElasticClient client)
        {

            var query = _criteria.GetSearchDescriptor();

            var results = await client.SearchAsync<BaseDocument>(query);

            return new AggregationQueryResults(results, _criteria);
        }
    }
    public class AggregationQuery<T> :
        ISeaqQuery<T>
        where T : BaseDocument
    {
        private AggregationQueryCriteria<T> _criteria;
        public ISeaqQueryCriteria<T> Criteria => _criteria;

        public AggregationQuery(
            AggregationQueryCriteria<T> criteria)
        {
            _criteria = criteria;
        }

        public ISeaqQueryResults<T> Execute(
            ElasticClient client)
        {

            var query = _criteria.GetSearchDescriptor();

            var results = client.Search<T>(query);

            return new AggregationQueryResults<T>(results, _criteria);
        }

        public async Task<ISeaqQueryResults<T>> ExecuteAsync(
            ElasticClient client)
        {

            var query = _criteria.GetSearchDescriptor();

            var results = await client.SearchAsync<T>(query);

            return new AggregationQueryResults<T>(results, _criteria);
        }
    }
}
