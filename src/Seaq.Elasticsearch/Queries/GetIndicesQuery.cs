using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Seaq.Elasticsearch.Queries
{
    public class GetIndicesQuery
    {
        public GetIndicesQuery(
            params string[] indexNames)
        {
            Request = new GetIndexRequest(Indices.Index(indexNames));
        }

        public GetIndexRequest Request { get; }
    }
}
