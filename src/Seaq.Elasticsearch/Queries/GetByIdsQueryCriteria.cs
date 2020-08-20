using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Nest;
using Seaq.Elasticsearch.Clusters;
using Seaq.Elasticsearch.Documents;

namespace Seaq.Elasticsearch.Queries
{
    public class GetByIdsQueryCriteria :
        ICriteria<IDocument>
    {
        public GetByIdsQueryCriteria(
            string storeIdName,
            params string[] documentIds)
        {
            StoreIdNames = ImmutableList<string>.Empty.Add(storeIdName);
            DocumentIds = documentIds;
            _paging = new Paging();
        }

        public GetByIdsQueryCriteria(
            string storeIdName,
            Paging paging = null,
            params string[] documentIds)
        {
            StoreIdNames = ImmutableList<string>.Empty.Add(storeIdName);
            DocumentIds = documentIds;
            _paging = paging ?? new Paging();
        }

        private Paging _paging { get; }
        public IPaging Paging => _paging;

        public ImmutableList<string> StoreIdNames { get; }

        public string[] DocumentIds { get; }

        public Func<SearchDescriptor<IDocument>, ISearchRequest> GetDescriptor()
        {
            return new Func<SearchDescriptor<IDocument>, ISearchRequest>(
                descriptor =>
                    descriptor
                        .Index(Indices.Index(StoreIdNames))
                        .Query(q => q.Ids(i => i.Values(DocumentIds)))

            );
        }

        public void CollectMetadataForQuery(Cluster cluster)
        {

        }
    }
}
