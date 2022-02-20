using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace seaq
{
    public class DefaultSeaqElasticsearchSerializer :
        SeaqElasticsearchSerializer
    {
        readonly Func<string, Type> TryGetCollectionType;

        static JsonSerializerOptions Options = new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, 
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };

    public DefaultSeaqElasticsearchSerializer(
            Func<string, Type> tryGetCollectionType)
        {
            TryGetCollectionType = tryGetCollectionType;
        }

        public override object Deserialize(Type type, Stream stream)
        {
            return DeserializeAsync(type, stream).Result;
        }

        public override T Deserialize<T>(Stream stream)
        {
            return DeserializeAsync<T>(stream).Result;            
        }

        protected override T Deserialize<T>(object data)
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(data, Options), Options);
        }

        public override async ValueTask<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default)
        {
            return await JsonSerializer.DeserializeAsync(stream, type, Options);
        }

        public override async ValueTask<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
        {
            Type type = null;

            var typ = typeof(T);

            if (typ == typeof(BaseDocument))
            {
                var obj = await JsonSerializer.DeserializeAsync<BaseDocument>(stream, Options);
                if (obj.Type is not null)
                    type = TryGetCollectionType(obj?.Type);
            }
            else
            {
                type = typ;
            }

            if (type is null)
            {
                return await JsonSerializer.DeserializeAsync<T>(stream, Options);
            }
            else
            {
                stream.Position = 0;
                return (T)(await JsonSerializer.DeserializeAsync(stream, type, Options));                
            }
        }

        public override void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None)
        {
            SerializeAsync<T>(data, stream, formatting).Wait();
        }

        public override async Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None, CancellationToken cancellationToken = default)
        {
            var type = TryGetCollectionType((data as BaseDocument)?.Type);
            if (type is null)
            {
                await JsonSerializer.SerializeAsync<T>(stream, data, Options);
            }
            else
            {
                await JsonSerializer.SerializeAsync(stream, data, type, Options);
            }
        }
    }
}
