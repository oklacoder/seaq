using Seaq.Elasticsearch.Documents;
using System;

namespace Seaq.Elasticsearch.Queries
{
    public class DefaultDocumentPropertyBuilder :
        IDocumentPropertyBuilder
    {
        public DefaultDocumentPropertyBuilder()
        {
            Type = typeof(IDocument);
        }

        public Type Type { get; }

        public string[] GetPropertyNames()
        {
            var fields =
                new[]
                {
                    nameof(IDocument.DocumentId),
                    nameof(IDocument.StoreId),
                    nameof(IDocument.Suggestions),
                    nameof(IDocument.Type),
                    nameof(IDocument.PrimaryDisplay),
                    nameof(IDocument.SecondaryDisplay)
                };

            return fields;
        }
    }
}
