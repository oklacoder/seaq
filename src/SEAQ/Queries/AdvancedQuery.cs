using System.Threading.Tasks;

namespace seaq
{
    public class AdvancedQuery<T> :
        ISeaqQuery<T>
    where T : class, IDocument
    {
        private AdvancedQueryCriteria<T> _criteria;
        public ISeaqQueryCriteria<T> Criteria => _criteria;

        public AdvancedQuery(
            AdvancedQueryCriteria<T> criteria)
        {
            _criteria = criteria;
        }

        public ISeaqQueryResults<T> Execute(Nest.ElasticClient client)
        {
            var results = client.Search<T>(_criteria.GetSearchDescriptor());

            return new AdvancedQueryResults<T>(results);
        }

        public async Task<ISeaqQueryResults<T>> ExecuteAsync(Nest.ElasticClient client)
        {
            var results = await client.SearchAsync<T>(_criteria.GetSearchDescriptor());

            return new AdvancedQueryResults<T>(results);
        }
    }
}
