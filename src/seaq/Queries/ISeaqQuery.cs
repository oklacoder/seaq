using Elastic.Clients.Elasticsearch;
using System.Threading.Tasks;

namespace seaq
{
    public interface ISeaqQuery 
    {
        public ISeaqQueryCriteria Criteria { get; }
        public ISeaqQueryResults Execute(ElasticsearchClient client);
        public Task<ISeaqQueryResults> ExecuteAsync(ElasticsearchClient client);
    }
    public interface ISeaqQuery<T>
        where T : BaseDocument
    {
        public ISeaqQueryCriteria<T> Criteria { get; }

        public ISeaqQueryResults<T> Execute(ElasticsearchClient client);
        public Task<ISeaqQueryResults<T>> ExecuteAsync(ElasticsearchClient client);
    }
}
