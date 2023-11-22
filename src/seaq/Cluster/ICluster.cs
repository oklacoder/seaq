using System.Collections.Generic;
using System.Threading.Tasks;

namespace seaq
{
    public interface ICluster
    {
        Task<bool> CommitAsync<T>(IEnumerable<T> documents) where T : class, IDocument;
        Task<bool> CommitAsync<T>(T document) where T : class, IDocument;
        Task<bool> DeleteAsync<T>(IEnumerable<T> documents) where T : BaseDocument;
        Task<bool> DeleteAsync<T>(T document) where T : BaseDocument;
        Task<ISeaqQueryResults<T>> QueryAsync<T>(ISeaqQuery<T> query) where T : BaseDocument;
    }
}