using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seaq
{
    public interface IQueryCriteria<T>
        where T : class, IDocument
    {
        Nest.SearchDescriptor<T> GetSearchDescriptor();
        public string[] Indices { get; }
        public int? Skip { get; }
        public int? Take { get; }
        IEnumerable<ISortField> SortFields { get; }
        IEnumerable<IReturnField> ReturnFields { get; }
        IEnumerable<IBucketField> BucketFields { get; }
    }
}
