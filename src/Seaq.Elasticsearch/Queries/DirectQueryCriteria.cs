using Nest;
using Seaq.Elasticsearch.Clusters;
using Seaq.Elasticsearch.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Seaq.Elasticsearch.Queries
{
    public class DirectQueryCriteria :
            ICriteria<IDocument>
    {
        private readonly Paging _paging;
        private readonly IFieldNameUtilities _fieldNameUtilities;
        private readonly SearchDescriptor<IDocument> Query;

        public IPaging Paging => _paging;
        public ImmutableList<string> StoreIdNames { get; }



        public DirectQueryCriteria(
            SearchDescriptor<IDocument> query,
            Paging paging = null)
        {
            Query = query;

            _paging = paging ?? new Paging();
        }

        public void CollectMetadataForQuery(Cluster cluster)
        {
            
        }

        public Func<SearchDescriptor<IDocument>, ISearchRequest> GetDescriptor()
        {
            return new Func<SearchDescriptor<IDocument>, ISearchRequest>(x => Query);
        }
    }
}
