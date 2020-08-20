using System;

namespace Seaq.Elasticsearch.Queries
{
    public interface IDocumentPropertyBuilder
    {
        Type Type { get; }

        string[] GetPropertyNames();
    }
}
