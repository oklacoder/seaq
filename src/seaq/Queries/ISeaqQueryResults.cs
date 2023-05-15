using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public class DefaultQueryResult :
        ISeaqQueryResult
    {
        /// <summary>
        /// Document Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Document's index
        /// </summary>
        public string Index { get; set; }
        /// <summary>
        /// Elaticsearch search score for this document in the provided query
        /// </summary>
        public double? Score { get; set; }
        /// <summary>
        /// Document version in this Elasticsearch index
        /// </summary>
        public long? Version { get; set; }
        /// <summary>
        /// Document body
        /// </summary>
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
        /// <summary>
        /// Document Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Document's index
        /// </summary>
        public string Index { get; set; }
        /// <summary>
        /// Elaticsearch search score for this document in the provided query
        /// </summary>
        public double? Score { get; set; }
        /// <summary>
        /// Document version in this Elasticsearch index
        /// </summary>
        public long? Version { get; set; }
        /// <summary>
        /// Document body
        /// </summary>
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
        /// <summary>
        /// Document Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Document's index
        /// </summary>
        public string Index { get; set; }
        /// <summary>
        /// Elaticsearch search score for this document in the provided query
        /// </summary>
        public double? Score { get; set; }
        /// <summary>
        /// Document version in this Elasticsearch index
        /// </summary>
        public long? Version { get; set; }
        /// <summary>
        /// Document body
        /// </summary>
        public BaseDocument Document { get; set; }
    }
    public interface ISeaqQueryResult<T>
        where T : BaseDocument
    {
        /// <summary>
        /// Document Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Document's index
        /// </summary>
        public string Index { get; set; }
        /// <summary>
        /// Elaticsearch search score for this document in the provided query
        /// </summary>
        public double? Score { get; set; }
        /// <summary>
        /// Document version in this Elasticsearch index
        /// </summary>
        public long? Version { get; set; }
        /// <summary>
        /// Document body
        /// </summary>
        public T Document { get; set; }
    }

    public interface ISeaqQueryResults
    {
        /// <summary>
        /// Collection of query result objects
        /// </summary>
        public IEnumerable<ISeaqQueryResult> Results { get; }
        /// <summary>
        /// Query execution duration
        /// </summary>
        long Took { get; }
        /// <summary>
        /// Total query results before paging applied
        /// </summary>
        long Total { get; }
        /// <summary>
        /// Additional information about this query execution
        /// </summary>
        public IEnumerable<string> Messages { get; }
    }
    public interface ISeaqQueryResults<T>
        where T : BaseDocument
    {
        /// <summary>
        /// Collection of query result objects
        /// </summary>
        public IEnumerable<ISeaqQueryResult<T>> Results { get; }
        /// <summary>
        /// Query execution duration
        /// </summary>
        long Took { get; }
        /// <summary>
        /// Total query results before paging applied
        /// </summary>
        long Total { get; }
        /// <summary>
        /// Additional information about this query execution
        /// </summary>
        public IEnumerable<string> Messages { get; }
    }
}
