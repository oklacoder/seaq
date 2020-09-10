using System.Dynamic;

namespace Seaq.Elasticsearch.Documents
{
    public class DynamicDocument : 
        DynamicObject, 
        IDocument
    {
        public DynamicDocument()
        {

        }
        public DynamicDocument(
            string documentId,
            string storeId,
            string type,
            string primaryDisplay,
            string secondaryDisplay,
            string[] suggestions)
        {
            DocumentId = documentId;
            StoreId = storeId;
            Type = type;
            PrimaryDisplay = primaryDisplay;
            SecondaryDisplay = secondaryDisplay;
            Suggestions = suggestions;
        }

        public string DocumentId { get; }

        public string StoreId { get; }

        public string Type { get; }

        public string PrimaryDisplay { get; }

        public string SecondaryDisplay { get; }

        public string[] Suggestions { get; }
    }
}
