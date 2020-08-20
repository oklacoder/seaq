using System;
using System.Collections.Generic;
using System.Text;

namespace Seaq.Elasticsearch.Documents
{
    public interface IDocument
    {
        string DocumentId { get; }
        string StoreId { get; }
        string Type { get; }
        string PrimaryDisplay { get; }
        string SecondaryDisplay { get; }
        string[] Suggestions { get; }

    }
}
