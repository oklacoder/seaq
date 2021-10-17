using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class DefaultQueryResult :
        ISeaqQueryResult
    {
        public string Id { get; set; }
        public string Index { get; set; }
        public double? Score { get; set; }
        public long? Version { get; set; }
        public BaseDocument Document { get; set; }

        public DefaultQueryResult() { }
        public DefaultQueryResult(
            BaseDocument document,
            string id = null,
            string index = null,
            double? score = null,
            long? version = null)
        {
            Document = document;
            Id = id ?? document.Id;
            Index = index;
            Score = score;
            Version = version;
        }

        public DefaultQueryResult(
            Nest.IHit<BaseDocument> hit)
        {
            Document = hit.Source;
            Id = hit.Id;
            Index = hit.Index;
            Score = hit.Score;
            Version = hit.Version;
        }
    }
    public class DefaultQueryResult<T> :
        ISeaqQueryResult<T>
        where T : BaseDocument
    {
        public string Id { get; set; }
        public string Index { get; set; }
        public double? Score { get; set; }
        public long? Version { get; set; }
        public T Document { get; set; }
        public DefaultQueryResult() { }
        public DefaultQueryResult(
            T document,
            string id = null,
            string index = null,
            double? score = null,
            long? version = null)
        {
            Document = document;
            Id = id ?? document.Id;
            Index = index;
            Score = score;
            Version = version;
        }

        public DefaultQueryResult(
            Nest.IHit<T> hit)
        {
            Document = hit.Source;
            Id = hit.Id;
            Index = hit.Index;
            Score = hit.Score;
            Version = hit.Version;
        }
    }
    public interface ISeaqQueryResult
    {
        public string Id { get; set; }
        public string Index { get; set; }
        public double? Score { get; set; }
        public long? Version { get; set; }

        public BaseDocument Document { get; set; }
    }
    public interface ISeaqQueryResult<T>
        where T : BaseDocument
    {
        public string Id { get; set; }
        public string Index { get; set; }
        public double? Score { get; set; }
        public long? Version { get; set; }

        public T Document { get; set; }
    }

    public interface ISeaqQueryResults
    {
        public IEnumerable<ISeaqQueryResult> Results { get; }
        long Took { get; }
        long Total { get; }
    }
    public interface ISeaqQueryResults<T>
        where T : BaseDocument
    {
        public IEnumerable<ISeaqQueryResult<T>> Results { get; }
        long Took { get; }
        long Total { get; }
    }
}
