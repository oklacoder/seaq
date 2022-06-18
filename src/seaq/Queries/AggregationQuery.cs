using Nest;
using System;
using System.Threading.Tasks;

namespace seaq
{
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

        public Task<ISeaqQueryResults<T>> ExecuteAsync(
            ElasticClient client)
        {
            throw new NotImplementedException();
        }
    }
}
