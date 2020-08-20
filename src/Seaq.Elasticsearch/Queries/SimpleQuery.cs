using Seaq.Elasticsearch.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Seaq.Elasticsearch.Queries
{
    public class SimpleQuery :
        IQuery<IDocument>
    {
        public ICriteria<IDocument> Criteria => _criteria;

        private SimpleQueryCriteria _criteria { get; }

        public SimpleQuery(
            SimpleQueryCriteria criteria)
        {
            _criteria = criteria;
        }
    }
}
