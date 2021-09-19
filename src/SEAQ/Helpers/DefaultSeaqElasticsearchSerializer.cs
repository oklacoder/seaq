using Elasticsearch.Net;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Utf8Json;

namespace seaq
{
    public class DefaultSeaqElasticsearchSerializer :
        ISeaqElasticsearchSerializer
    {
        readonly Func<string, Type, bool> TryGetCollectionType;

        public DefaultSeaqElasticsearchSerializer(
            Func<string, Type, bool> tryGetCollectionType)
        {
            TryGetCollectionType = tryGetCollectionType;
        }

        public object Deserialize(Type type, Stream stream)
        {
            return JsonSerializer.NonGeneric.Deserialize(type, stream, Utf8Json.Resolvers.StandardResolver.CamelCase);
        }

        public T Deserialize<T>(Stream stream)
        {
            return JsonSerializer.Deserialize<T>(stream, Utf8Json.Resolvers.StandardResolver.CamelCase);
        }

        public T Deserialize<T>(object data)
        {
            var t = typeof(T);
            var r = JsonSerializer.NonGeneric.Serialize(data);
            return JsonSerializer.Deserialize<T>(JsonSerializer.NonGeneric.Serialize(data), Utf8Json.Resolvers.StandardResolver.CamelCase);
        }

        public Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default)
        {
            return JsonSerializer.NonGeneric.DeserializeAsync(type, stream, Utf8Json.Resolvers.StandardResolver.CamelCase);
        }

        public Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
        {
            return JsonSerializer.DeserializeAsync<T>(stream, Utf8Json.Resolvers.StandardResolver.CamelCase);
        }

        public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None)
        {
            JsonSerializer.Serialize<T>(stream, data, Utf8Json.Resolvers.StandardResolver.CamelCase);
        }

        public Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None, CancellationToken cancellationToken = default)
        {
            return JsonSerializer.SerializeAsync<T>(stream, data, Utf8Json.Resolvers.StandardResolver.CamelCase);
        }
    }
}
