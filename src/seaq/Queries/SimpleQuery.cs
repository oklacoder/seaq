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

            return new SimpleQueryResults(results);
        }

        public async Task<ISeaqQueryResults> ExecuteAsync(Nest.ElasticClient client)
        {
            var query = Criteria.GetSearchDescriptor();

            Log.Verbose("Executing query with params: {0}",
                client.RequestResponseSerializer.SerializeToString(query));

            var results = await client.SearchAsync<BaseDocument>(query);

            return new SimpleQueryResults(results);
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

            return new SimpleQueryResults<T>(results);
        }

        public async Task<ISeaqQueryResults<T>> ExecuteAsync(Nest.ElasticClient client)
        {
            var query = Criteria.GetSearchDescriptor();

            Log.Verbose("Executing query with params: {0}",
                client.RequestResponseSerializer.SerializeToString(query));

            var results = await client.SearchAsync<T>(query);

            return new SimpleQueryResults<T>(results);
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
