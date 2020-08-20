using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Newtonsoft.Json;
using Seaq.Elasticsearch.Documents;

namespace Seaq.Elasticsearch.Clusters
{
    public class NewtonsoftElasticsearchSerializer : 
        ISeaqElasticsearchSerializer
    {
        readonly Func<string, Type> TryGetStoreType;
        Newtonsoft.Json.Serialization.DefaultContractResolver defaultResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver() { NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy() };

        public NewtonsoftElasticsearchSerializer(
            Func<string, Type> tryGetStoreType)
        {
            TryGetStoreType = tryGetStoreType;
        }

        public T Deserialize<T>(object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var returnValue = JsonConvert.DeserializeObject<T>(json);
            return returnValue;
        }

        public object Deserialize(Type type, Stream stream)
        {
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);

            var json = System.Text.Encoding.UTF8.GetString(buffer);
            var result = JsonConvert.DeserializeObject(json, type);
            return result;
        }

        public T Deserialize<T>(Stream stream)
        {
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);

            var json = System.Text.Encoding.UTF8.GetString(buffer);

            Type type;
            if (typeof(T) == typeof(ISkinnyDocument))
            {
                type = typeof(SkinnyDocument);
            }
            else
            {
                var iDoc = JsonConvert.DeserializeObject<SkinnyDocument>(json);

                //type = TryGetStoreType(iDoc.StoreId);
                type = TryGetStoreType(iDoc.Type);
            }

            var result = JsonConvert.DeserializeObject(json, type);

            return (T)result;
        }

        public Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default)
        {
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);

            var json = System.Text.Encoding.UTF8.GetString(buffer);

            return Task.Run(() => JsonConvert.DeserializeObject(json, type));
        }

        public Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
        {
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);

            var json = System.Text.Encoding.UTF8.GetString(buffer);

            return Task.Run(() => JsonConvert.DeserializeObject<T>(json));
        }

        public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None)
        {
            var json = JsonConvert.SerializeObject(data, new JsonSerializerSettings() { ContractResolver = defaultResolver });
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            stream.Write(bytes);
        }

        public Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None, CancellationToken cancellationToken = default)
        {
            var json = JsonConvert.SerializeObject(data, new JsonSerializerSettings() { ContractResolver = defaultResolver });
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            
            return stream.WriteAsync(bytes).AsTask();
        }
    }
}
