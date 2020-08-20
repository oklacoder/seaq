using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Seaq.Elasticsearch.Documents
{
    [DataContract]
    public sealed class SkinnyDocument :
            ISkinnyDocument
    {
        public SkinnyDocument(
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

        [DataMember(Name = nameof(DocumentId))]
        public string DocumentId { get; }

        [DataMember(Name = nameof(StoreId))]
        public string StoreId { get; }

        [DataMember(Name = nameof(Type))]
        public string Type { get; }

        [DataMember(Name = nameof(PrimaryDisplay))]
        public string PrimaryDisplay { get; }

        [DataMember(Name = nameof(SecondaryDisplay))]
        public string SecondaryDisplay { get; }

        [DataMember(Name = nameof(Suggestions))]
        public string[] Suggestions { get; }
    }
}
