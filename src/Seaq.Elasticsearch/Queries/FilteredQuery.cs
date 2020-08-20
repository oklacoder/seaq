using Seaq.Elasticsearch.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Seaq.Elasticsearch.Queries
{
    public class FilteredQuery :
        IQuery<IDocument>
    {
        private FilteredQueryCriteria _criteria { get; }
        public ICriteria<IDocument> Criteria => _criteria;


        public FilteredQuery(
            FilteredQueryCriteria criteria)
        {
            _criteria = criteria;
        }
    }
}
