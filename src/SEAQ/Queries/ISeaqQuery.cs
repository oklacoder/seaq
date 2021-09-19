using System.Threading.Tasks;

namespace seaq
{
    public interface ISeaqQuery<T>
        where T : class, IDocument
    {
        public ISeaqQueryCriteria<T> Criteria { get; }

        public ISeaqQueryResults<T> Execute(Nest.ElasticClient client);
        public Task<ISeaqQueryResults<T>> ExecuteAsync(Nest.ElasticClient client);
    }
}
