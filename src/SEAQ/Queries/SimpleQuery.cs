using System.Threading.Tasks;

namespace seaq
{
    public class SimpleQuery<T> :
        ISeaqQuery<T>
        where T : class, IDocument
    {
        private SimpleQueryCriteria<T> _criteria;
        public ISeaqQueryCriteria<T> Criteria => _criteria;


        public ISeaqQueryResults<T> Execute(Nest.ElasticClient client)
        {
            var results = client.Search<T>(Criteria.GetSearchDescriptor());

            return new SimpleQueryResults<T>(results);
        }

        public async Task<ISeaqQueryResults<T>> ExecuteAsync(Nest.ElasticClient client)
        {
            var results = await client.SearchAsync<T>(Criteria.GetSearchDescriptor());

            return new SimpleQueryResults<T>(results);
        }

        public SimpleQuery(
            SimpleQueryCriteria<T> criteria)
        {
            _criteria = criteria;
        }
    }
}
