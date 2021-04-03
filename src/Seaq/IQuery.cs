using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seaq
{
    public interface IQuery<T>
        where T : class, IDocument
    {
        public IQueryCriteria<T> Criteria { get; }

        public IQueryResults<T> Execute(Nest.ElasticClient client);
        public Task<IQueryResults<T>> ExecuteAsync(Nest.ElasticClient client);
    }
}
