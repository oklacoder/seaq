using Seaq.Elasticsearch.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Seaq.Elasticsearch.Queries
{
    public class FieldValuesQuery :
        IQuery<IDocument>
    {
        private FieldValuesQueryCriteria _criteria { get; }
        public ICriteria<IDocument> Criteria => _criteria;
        
        public FieldValuesQuery(
            FieldValuesQueryCriteria criteria)
        {
            _criteria = criteria;
        }
    }
}
