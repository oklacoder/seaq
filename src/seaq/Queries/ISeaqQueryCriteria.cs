﻿using Elastic.Clients.Elasticsearch;
using System.Collections.Generic;
using System.Linq;

namespace seaq
{
    public interface ISeaqQueryCriteria 
    {
        /// <summary>
        /// Full dotnet type name of desired return objects
        /// </summary>
        public string Type { get; }
        SearchRequestDescriptor<BaseDocument> GetSearchDescriptor();
        void ApplyClusterSettings(Cluster cluster);
        /// <summary>
        /// Specify which indices to query.  If empty or null, query will default to the default index for the provided type.
        /// </summary>
        public string[] Indices { get; }
        /// <summary>
        /// Used for paging.  Note that this is only deterministic if consistent sort fields are provided on each related query.
        /// </summary>
        public int? Skip { get; }
        /// <summary>
        /// Used for paging.  Note that this is only deterministic if consistent sort fields are provided on each related query.
        /// </summary>
        public int? Take { get; }
        /// <summary>
        /// Collection of ISortField objects used to order the query results
        /// </summary>
        IEnumerable<ISortField> SortFields { get; }
        /// <summary>
        /// Collection of IReturnField objects used to limit tthe fields included in the query results
        /// </summary>
        IEnumerable<IReturnField> ReturnFields { get; }
        /// <summary>
        /// Collection of IBucketField objects used to control returned terms aggregations for further filtering
        /// </summary>
        IEnumerable<IBucketField> BucketFields { get; }
        /// <summary>
        /// Collection of strings used to control which fields are used to calculate score boosting
        /// </summary>
        IEnumerable<string> BoostedFields { get; }
    }
    public interface ISeaqQueryCriteria<T>
        where T : BaseDocument
    {
        SearchRequestDescriptor<T> GetSearchDescriptor();
        void ApplyClusterSettings(Cluster cluster);
        /// <summary>
        /// Specify which indices to query.  If empty or null, query will default to the default index for the provided type.
        /// </summary>
        public string[] Indices { get; }
        /// <summary>
        /// Used for paging.  Note that this is only deterministic if consistent sort fields are provided on each related query.
        /// </summary>
        public int? Skip { get; }
        /// <summary>
        /// Used for paging.  Note that this is only deterministic if consistent sort fields are provided on each related query.
        /// </summary>
        public int? Take { get; }
        /// <summary>
        /// Collection of ISortField objects used to order the query results
        /// </summary>
        IEnumerable<ISortField> SortFields { get; }
        /// <summary>
        /// Collection of IReturnField objects used to limit tthe fields included in the query results
        /// </summary>
        IEnumerable<IReturnField> ReturnFields { get; }
        /// <summary>
        /// Collection of IBucketField objects used to control returned terms aggregations for further filtering
        /// </summary>
        IEnumerable<IBucketField> BucketFields { get; }
        /// <summary>
        /// Collection of strings used to control which fields are used to calculate score boosting
        /// </summary>
        IEnumerable<string> BoostedFields { get; }
    }
}
