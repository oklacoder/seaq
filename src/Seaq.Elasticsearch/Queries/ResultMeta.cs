using Nest;
using System.Linq;
namespace Seaq.Elasticsearch.Queries
{
    public class DefaultResultMeta : 
        IResultMeta
    {
        public long Took { get; }
        public string[] ResultDocumentStores { get; }

        public DefaultResultMeta(ISearchResponse<object> searchResponse)
        {
            Took = searchResponse.Took;
            ResultDocumentStores = searchResponse.Hits.Select(x => x.Index).Distinct().ToArray();
        }
    }
}
