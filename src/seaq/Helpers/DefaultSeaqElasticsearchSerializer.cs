using Elasticsearch.Net;
using Serilog;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace seaq
{
    public class DefaultSeaqElasticsearchSerializer :
        ISeaqElasticsearchSerializer
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

        public object Deserialize(Type type, Stream stream)
        {
            return DeserializeAsync(type, stream).Result;
        }

        public T Deserialize<T>(Stream stream)
        {
            return DeserializeAsync<T>(stream).Result;            
        }

        public T Deserialize<T>(object data)
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(data, Options), Options);
        }

        public async Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default)
        {
            return await JsonSerializer.DeserializeAsync(stream, type, Options);
        }

        public async Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
        {
            Type type = null;

            var typ = typeof(T);

            var t = typeof(BaseDocument);
            if (typ == t)
            {
                var src = await JsonSerializer.DeserializeAsync(stream, t, Options);
                var obj = (BaseDocument)src;
                if (obj.Type is not null)
                    type = TryGetCollectionType(obj?.Type);
                if (type == typeof(BaseDocument))
                {
                    Log.Warning("Unable to match {0}-{1} to a loaded assembly type.  It will be returned as a {2}, likely missing some data that exists on the server.",
                        obj.Type, obj.Id, typeof(BaseDocument).FullName);
                    return (T)src;
                }
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

        public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None)
        {
            SerializeAsync<T>(data, stream, formatting).Wait();
        }

        public async Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None, CancellationToken cancellationToken = default)
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
