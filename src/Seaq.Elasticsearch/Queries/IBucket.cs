using System.Collections.Immutable;

namespace Seaq.Elasticsearch.Queries
{
    public interface IBucket
    {
        string Key { get; }
        ImmutableList<IBucketValue> Values { get; }
    }
}
