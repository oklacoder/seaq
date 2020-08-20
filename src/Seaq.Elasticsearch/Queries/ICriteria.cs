using Nest;
using Seaq.Elasticsearch.Clusters;
using Seaq.Elasticsearch.Documents;
using System;
using System.Collections.Immutable;

namespace Seaq.Elasticsearch.Queries
{
    public interface ICriteria<TDocument>
        where TDocument : class, IDocument
    {
        IPaging Paging { get; }
        ImmutableList<string> StoreIdNames { get; }

        void CollectMetadataForQuery(Cluster cluster);

        Func<SearchDescriptor<TDocument>, ISearchRequest> GetDescriptor();
    }
}
