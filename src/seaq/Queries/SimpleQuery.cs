using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using Elasticsearch.Net;
using Serilog;

namespace seaq
{
    [DataContract]
    public class SimpleQuery :
        ISeaqQuery
    {
        [DataMember(Name = "criteria")]
        [JsonPropertyName("criteria")]
        public SimpleQueryCriteria  _criteria { get; set; }
        public ISeaqQueryCriteria Criteria => _criteria;

        public ISeaqQueryResults Execute(Nest.ElasticClient client)
        {
            var query = Criteria.GetSearchDescriptor();

            Log.Verbose("Executing query with params: {0}", 
                client.RequestResponseSerializer.SerializeToString(query));

            var results = client.Search<BaseDocument>(query);

            return new SimpleQueryResults(results, _criteria.DeprecatedIndexTargets);
        }

        public async Task<ISeaqQueryResults> ExecuteAsync(Nest.ElasticClient client)
        {
            var query = Criteria.GetSearchDescriptor();

            Log.Verbose("Executing query with params: {0}",
                client.RequestResponseSerializer.SerializeToString(query));

            var results = await client.SearchAsync<BaseDocument>(query);

            if (results.IsValid is not true)
            {
                Log.Error("Error processing query:");
                Log.Error(results.DebugInformation);
                if (results.OriginalException is not null)
                {
                    Log.Error(results.OriginalException.Message);
                    Log.Error(results.OriginalException.StackTrace);
                }
                if (results.ServerError?.Error?.Reason is not null)
                {
                    Log.Error(results.ServerError?.Error?.Reason);
                }
            }

            return new SimpleQueryResults(results, _criteria.DeprecatedIndexTargets);
        }

        public SimpleQuery(
            SimpleQueryCriteria criteria)
        {
            _criteria = criteria;
        }
        public SimpleQuery()
        {

        }
    }
    [DataContract]
    public class SimpleQuery<T> :
        ISeaqQuery<T>
        where T : BaseDocument
    {
        [DataMember(Name = "criteria")]
        [JsonPropertyName("criteria")]
        public SimpleQueryCriteria<T> _criteria { get; set; }
        public ISeaqQueryCriteria<T> Criteria => _criteria;


        public ISeaqQueryResults<T> Execute(Nest.ElasticClient client)
        {
            var query = Criteria.GetSearchDescriptor();

            Log.Verbose("Executing query with params: {0}",
                client.RequestResponseSerializer.SerializeToString(query));

            var results = client.Search<T>(query);

            return new SimpleQueryResults<T>(results, _criteria.DeprecatedIndexTargets);
        }

        public async Task<ISeaqQueryResults<T>> ExecuteAsync(Nest.ElasticClient client)
        {
            var query = Criteria.GetSearchDescriptor();

            Log.Verbose("Executing query with params: {0}",
                client.RequestResponseSerializer.SerializeToString(query));

            var results = await client.SearchAsync<T>(query);

            if (results.IsValid is not true)
            {
                Log.Error("Error processing query:");
                Log.Error(results.DebugInformation);
                if (results.OriginalException is not null)
                {
                    Log.Error(results.OriginalException.Message);
                    Log.Error(results.OriginalException.StackTrace);
                }
                if (results.ServerError?.Error?.Reason is not null)
                {
                    Log.Error(results.ServerError?.Error?.Reason);
                }
            }

            return new SimpleQueryResults<T>(results, _criteria.DeprecatedIndexTargets);
        }

        public SimpleQuery(
            SimpleQueryCriteria<T> criteria)
        {
            _criteria = criteria;
        }
        public SimpleQuery()
        {

        }
    }
}
