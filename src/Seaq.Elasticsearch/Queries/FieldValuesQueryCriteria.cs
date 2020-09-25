using Nest;
using Seaq.Elasticsearch.Clusters;
using Seaq.Elasticsearch.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Seaq.Elasticsearch.Queries
{
    public class FieldValuesQueryCriteria :
            ICriteria<IDocument>
    {
        private readonly Paging _paging;
        private readonly IFieldNameUtilities _fieldNameUtilities;

        public FieldValuesQueryCriteria(
            IEnumerable<string> storeIdNames,
            string fieldName,
            string queryText,
            Paging paging = null,
            IFieldNameUtilities fieldNameUtilities = null)
        {
            StoreIdNames = ImmutableList<string>.Empty.AddRange(storeIdNames);
            FieldName = fieldName;
            QueryText = queryText;
            _paging = paging ?? new Paging();
            this._fieldNameUtilities = fieldNameUtilities ?? new DefaultFieldNameUtilities();
        }

        public IPaging Paging => _paging;
        public ImmutableList<string> StoreIdNames { get; }
        public string FieldName { get; }
        public string QueryText { get; }

        public string[] AggregatableFields { get; set; }

        public void CollectMetadataForQuery(Cluster cluster)
        {
            var schemas = cluster.GetStoreSchemas(StoreIdNames.ToArray());

            AggregatableFields = 
                schemas?.SelectMany(x => 
                    x.GetSortableFieldNames(
                        _fieldNameUtilities.RemoveKnownPropertySuffixesFromPropertyName(FieldName)))?.ToArray() ?? new string[] { };
            //next step is to swap this to allow for hitting nested fields
        }

        public Func<SearchDescriptor<IDocument>, ISearchRequest> GetDescriptor()
        {
            var returnValue =
                new Func<SearchDescriptor<IDocument>, ISearchRequest>(
                    descriptor => GetSearchDescriptor<IDocument>());
            return returnValue;
        }

        private SearchDescriptor<TDocument> GetSearchDescriptor<TDocument>()
                                                            where TDocument : class, IDocument
        {
            return BuildFieldValueSearch<TDocument>(AggregatableFields);
        }

        private SearchDescriptor<T> BuildFieldValueSearch<T>(
            IEnumerable<string> aggs) where T : class
        {
            var search = new SearchDescriptor<T>();
            var indices = StoreIdNames;
            search
                .Index(Indices.Index(indices))
                .Query(q => BuildQuery<T>())
                .Aggregations(agg => GetFieldValuesAggregations<T>(aggs))
                .Size(0);

            return search;
        }

        private QueryContainerDescriptor<T> BuildQuery<T>() where T : class
        {
            var search = new QueryContainerDescriptor<T>();

            //search.Bool(b => b
            //    .Filter(f => f
            //        .Bool(fb =>
            //            GetBoolQueryForFieldValues<T>())));

            return search;
        }

        private BoolQueryDescriptor<T> GetBoolQueryForFieldValues<T>() 
            where T : class
        {
            var @bool = new BoolQueryDescriptor<T>();

            
            @bool.Must(m => m
                .QueryString(q => q
                .Query(QueryText)));

            return @bool;
        }

        private AggregationContainerDescriptor<T> GetFieldValuesAggregations<T>(
            IEnumerable<string> aggs) where T : class
        {
            var returnVal = new AggregationContainerDescriptor<T>();

            foreach (var key in aggs)
            {
                returnVal.Terms(key, t => t
                    .Field(key)
                    .Size(10)
                    .MinimumDocumentCount(1)
                    .Include(string.IsNullOrWhiteSpace(QueryText) ? ".*" : $".*{QueryText.ToLowerInvariant()}.*")//need this to be case insensitive
                    .Order(o => o.KeyAscending().CountDescending()));
            }

            return returnVal;
        }
        
    }
}
