using Nest;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace seaq
{
    public class BaseDocument :
        IDocument
    {
        [DataMember(Name = "id")]
        [JsonPropertyName("id")]
        public virtual string Id { get; set; }

        [DataMember(Name = "indexName")]
        [JsonPropertyName("indexName")]
        public virtual string IndexName { get; set; }

        [DataMember(Name = "type")]
        [JsonPropertyName("type")]
        public virtual string Type { get; set; }

        [DataMember(Name = "indexAsType")]
        [JsonPropertyName("indexAsType")]
        public virtual string IndexAsType { get; set; }

        public BaseDocument()
        {

        }
    }
}
