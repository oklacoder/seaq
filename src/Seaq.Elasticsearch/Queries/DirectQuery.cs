using Seaq.Elasticsearch.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Seaq.Elasticsearch.Queries
{
    public class DirectQuery :
        IQuery<IDocument>
    {
        public DirectQueryCriteria _criteria { get; }

        public DirectQuery(DirectQueryCriteria criteria)
        {
            _criteria = criteria;
        }

        public ICriteria<IDocument> Criteria => _criteria;
    }
}
