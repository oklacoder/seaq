using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seaq
{
    public class AdvancedQuery<T> :
        IQuery<T>
        where T : class, IDocument
    {
        private AdvancedQueryCriteria<T> _criteria;
        public IQueryCriteria<T> Criteria => _criteria;

        public AdvancedQuery(
            AdvancedQueryCriteria<T> criteria)
        {
            _criteria = criteria;
        }

        public IQueryResults<T> Execute(Nest.ElasticClient client)
        {
            var results = client.Search<T>(_criteria.GetSearchDescriptor());

            return new AdvancedQueryResults<T>(results);
        }

        public async Task<IQueryResults<T>> ExecuteAsync(Nest.ElasticClient client)
        {
            var results = await client.SearchAsync<T>(_criteria.GetSearchDescriptor());

            return new AdvancedQueryResults<T>(results);
        }
    }
}
