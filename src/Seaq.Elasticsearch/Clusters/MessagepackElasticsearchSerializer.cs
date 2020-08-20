using Elasticsearch.Net;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Internal;
using MessagePack.Resolvers;
using Seaq.Elasticsearch.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Seaq.Elasticsearch.Clusters
{
    //public class MessagePackElasticsearchSerializer
    //       : IElasticsearchSerializer
    //{

    //    Dictionary<string, Type> _typeCache;

    //    Func<string, Type> TryGetStoreType;

    //    private readonly IFormatterResolver _resolver;
    //    private readonly MessagePackSerializerOptions _serializerOptions;

    //    public MessagePackElasticsearchSerializer(
    //        Func<string, Type> tryGetStoreType)
    //    {
    //        throw new NotImplementedException("Not working correctly yet.  Still making efforts at properly robust datetime serialization.");
    //        _resolver = CompositeResolver.Create(
    //            new[] { new NativeDateTimeFormatter() },
    //            new[] { StandardResolver.Instance }
    //            );

    //        _serializerOptions = MessagePackSerializerOptions.Standard.WithResolver(_resolver);


    //        _typeCache = new Dictionary<string, Type>();
    //        TryGetStoreType = tryGetStoreType;
    //    }

    //    public T Deserialize<T>(object data)
    //    {
    //        return MessagePackSerializer.Deserialize<T>(MessagePackSerializer.Serialize(data), _serializerOptions);
    //    }

    //    public object Deserialize(Type type, Stream stream)
    //    {
    //        var buffer = new byte[stream.Length];
    //        stream.Read(buffer, 0, (int)stream.Length);

    //        var json = System.Text.Encoding.UTF8.GetString(buffer);
    //        var result = MessagePackSerializer.Deserialize(type, MessagePackSerializer.ConvertFromJson(json), _serializerOptions);
    //        return result;
    //    }

    //    public T Deserialize<T>(Stream stream)
    //    {
    //        var buffer = new byte[stream.Length];
    //        stream.Read(buffer, 0, (int)stream.Length);

    //        var json = System.Text.Encoding.UTF8.GetString(buffer);

    //        Type type;
    //        if (typeof(T) == typeof(ISkinnyDocument))
    //        {
    //            type = typeof(SkinnyDocument);
    //        }
    //        else
    //        {
    //            var iDoc = MessagePackSerializer.Deserialize<dynamic>(MessagePackSerializer.ConvertFromJson(json), _serializerOptions);
    //            var field = nameof(IDocument.StoreId);
    //            //type = _typeCache[iDoc[field]];
    //            type = TryGetStoreType(iDoc[field]);
    //        }


    //        var result = MessagePackSerializer.Deserialize(type, MessagePackSerializer.ConvertFromJson(json));

    //        return (T)result;
    //    }

    //    public Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default)
    //    {
    //        return MessagePackSerializer.DeserializeAsync(type, stream).AsTask();
    //    }

    //    public Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
    //    {
    //        return MessagePackSerializer.DeserializeAsync<T>(stream).AsTask();
    //    }

    //    public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None)
    //    {
    //        var json = MessagePackSerializer.ConvertToJson(MessagePackSerializer.Serialize(typeof(T), data, _serializerOptions), _serializerOptions);
    //        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

    //        stream.Write(bytes);
    //    }

    //    public Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None, CancellationToken cancellationToken = default)
    //    {
    //        var json = MessagePackSerializer.ConvertToJson(MessagePackSerializer.Serialize(typeof(T), data, _serializerOptions), _serializerOptions);
    //        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            
    //        return stream.WriteAsync(bytes).AsTask();
    //    }
    //}
}
