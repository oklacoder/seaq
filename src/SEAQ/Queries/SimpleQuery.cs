using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using Elasticsearch.Net;

namespace seaq
{
    [DataContract]
    public class SimpleQuery :
        SimpleQuery<BaseDocument>
    {
        [DataMember(Name = "criteria")]
        [JsonPropertyName("criteria")]
        public SimpleQueryCriteria  _criteria { get; set; }
        public override ISeaqQueryCriteria<BaseDocument> Criteria => _criteria;

        public override ISeaqQueryResults<BaseDocument> Execute(Nest.ElasticClient client)
        {
            var query = Criteria.GetSearchDescriptor();
            var str = client.RequestResponseSerializer.SerializeToString(query);

            var results = client.Search<BaseDocument>(query);

            return new SimpleQueryResults<BaseDocument>(results);
        }

        public override async Task<ISeaqQueryResults<BaseDocument>> ExecuteAsync(Nest.ElasticClient client)
        {
            var results = await client.SearchAsync<BaseDocument>(Criteria.GetSearchDescriptor());

            return new SimpleQueryResults<BaseDocument>(results);
        }

        public SimpleQuery(
            SimpleQueryCriteria criteria)
            : base(criteria)
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
        public virtual ISeaqQueryCriteria<T> Criteria => _criteria;


        public virtual ISeaqQueryResults<T> Execute(Nest.ElasticClient client)
        {
            var results = client.Search<T>(Criteria.GetSearchDescriptor());

            return new SimpleQueryResults<T>(results);
        }

        public virtual async Task<ISeaqQueryResults<T>> ExecuteAsync(Nest.ElasticClient client)
        {
            var results = await client.SearchAsync<T>(Criteria.GetSearchDescriptor());

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
