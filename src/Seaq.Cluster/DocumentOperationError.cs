namespace Seaq.Clusters
{
    public class DocumentOperationError
    {
        public string CollectionName { get; set; }

        public string DocumentId { get; set; }
        public string Error { get; set; }

        public DocumentOperationError(
            string collectionName,
            string error,
            string documentId)
        {
            CollectionName = collectionName;
            Error = error;
            DocumentId = documentId;
        }
    }

}
