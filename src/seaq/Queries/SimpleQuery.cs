using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using Serilog;
using Elastic.Clients.Elasticsearch;

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

        public ISeaqQueryResults Execute(ElasticsearchClient client)
        {
            var results = client.Search<BaseDocument>(x => _criteria.GetSearchDescriptor());

            return new SimpleQueryResults(results, _criteria.DeprecatedIndexTargets);
        }

        public async Task<ISeaqQueryResults> ExecuteAsync(ElasticsearchClient client)
        {
            var results = await client.SearchAsync<BaseDocument>(x => _criteria.GetSearchDescriptor());

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


        public ISeaqQueryResults<T> Execute(ElasticsearchClient client)
        {
            var results = client.Search<T>(x => _criteria.GetSearchDescriptor());

            return new SimpleQueryResults<T>(results, _criteria.DeprecatedIndexTargets);
        }

        public async Task<ISeaqQueryResults<T>> ExecuteAsync(ElasticsearchClient client)
        {
            var results = await client.SearchAsync<T>(x => _criteria.GetSearchDescriptor());

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
