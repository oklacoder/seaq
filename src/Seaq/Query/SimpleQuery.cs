using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seaq
{
    public class SimpleQuery<T> :
        IQuery<T>
        where T : class, IDocument
    {
        private SimpleQueryCriteria<T> _criteria;
        public IQueryCriteria<T> Criteria => _criteria;


        public IQueryResults<T> Execute(Nest.ElasticClient client)
        {
            var results = client.Search<T>(Criteria.GetSearchDescriptor());

            return new SimpleQueryResults<T>(results);
        }

        public async Task<IQueryResults<T>> ExecuteAsync(Nest.ElasticClient client)
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
