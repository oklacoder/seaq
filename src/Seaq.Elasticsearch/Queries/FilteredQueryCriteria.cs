using Nest;
using Seaq.Elasticsearch.Clusters;
using Seaq.Elasticsearch.Documents;
using Seaq.Elasticsearch.Queries.Comparators;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Seaq.Elasticsearch.Queries
{
    public class FilteredQueryCriteria :
            ICriteria<IDocument>
    {
        public static string aggregateKey = "fields";
        private readonly Paging _paging;
        private readonly DefaultQueryBuilder _queryBuilder;
        private readonly IFieldNameUtilities _fieldNameUtilities;
        private readonly bool _restrictBucketsToInputFilters;
        private readonly IDocumentPropertyBuilder _propertyBuilder;

        public string[] AggregatableFields { get; set; }
        public string[] BoostableFields { get; set; }


        public FilteredQueryCriteria(
            IEnumerable<string> storeIdNames,
            IEnumerable<QueryFilter> filters,
            Paging paging = null,
            DefaultQueryBuilder queryBuilder = null,
            IDocumentPropertyBuilder propertyBuilder = null,
            IFieldNameUtilities fieldNameUtilities = null,
            bool restrictBucketsToInputFilters = false)
        {
            StoreIdNames = ImmutableList<string>.Empty.AddRange(storeIdNames);

            Filters = ImmutableList<QueryFilter>.Empty.AddRange(filters);
            
            _paging = paging ?? new Paging();
            
            _queryBuilder = queryBuilder ?? new DefaultQueryBuilder();
            _propertyBuilder = propertyBuilder ?? new DefaultDocumentPropertyBuilder();
            _fieldNameUtilities = fieldNameUtilities ?? new DefaultFieldNameUtilities();
            this._restrictBucketsToInputFilters = restrictBucketsToInputFilters;
        }

        public ImmutableList<QueryFilter> Filters { get; }

        public IPaging Paging => _paging;

        public ImmutableList<string> StoreIdNames { get; }

        public Func<SearchDescriptor<IDocument>, ISearchRequest> GetDescriptor()
        {
            var returnValue =
                new Func<SearchDescriptor<IDocument>, ISearchRequest>(
                    descriptor => GetSearchDescriptor<IDocument>());
            return returnValue;
        }

        public void CollectMetadataForQuery(Cluster cluster)
        {
            var schemas = cluster.GetStoreSchemas(StoreIdNames.ToArray());

            AggregatableFields = 
                _restrictBucketsToInputFilters == true ?
                schemas?.SelectMany(x => x.GetAggregatableFieldNames(_fieldNameUtilities, Filters.Select(x => x.Field).ToArray()))?.ToArray() ?? new string[] { } :
                schemas?.SelectMany(x => x.GetAggregatableFieldNames())?.ToArray() ?? new string[] { };
            BoostableFields = schemas?.SelectMany(x => x.GetAllBoostedFieldNames())?.ToArray() ?? new string[] { };
        }

        private SearchDescriptor<T> BuildFilteredSearch<T>()
            where T : class
        {
            var search = new SearchDescriptor<T>();
            var indices = StoreIdNames;

            search
                .Index(Indices.Index(indices))
                .Query(q => BuildQuery<T>())
                .Sort(s => GetSortDescriptorForFilters<T>())
                .From(Paging.PageSize * (Paging.CurrentPage - 1))
                .Size(Paging.PageSize);

            return search;
        }

        private SearchDescriptor<T> BuildFilteredSearch<T>(
            IEnumerable<string> aggs) where T : class
        {
            var search = new SearchDescriptor<T>();
            var indices = StoreIdNames;
            search
                .Index(Indices.Index(indices))
                .Query(q => BuildQuery<T>())
                .Aggregations(agg => GetFilterAggregations<T>(aggs))
                .Sort(s => GetSortDescriptorForFilters<T>())
                .Skip(Paging.PageSize * (Paging.CurrentPage - 1))
                .Size(Paging.PageSize);

            return search;
        }

        private QueryContainerDescriptor<T> BuildQuery<T>() where T : class
        {
            var search = new QueryContainerDescriptor<T>();

            var toQuery = Filters.Where(p => !string.IsNullOrWhiteSpace(p.Field) && !string.IsNullOrWhiteSpace(p.Value));

            if (toQuery.Any())
            {
                search.Bool(b =>b
                    .Should(sh => sh
                        .MultiMatch(mm => mm
                            .Operator(Operator.Or)
                            .Type(TextQueryType.PhrasePrefix)
                            .Fields(BoostableFields)))
                    .Filter(f => f
                        .Bool(fb =>
                            GetBoolQueryForFilters<T>(Filters))));
            }

            return search;
        }

        private BoolQueryDescriptor<T> GetBoolQueryForFilters<T>(
            IEnumerable<QueryFilter> filters) where T : class
        {
            var @bool = new BoolQueryDescriptor<T>();

            var query = new List<QueryContainer>();

            foreach (var filter in filters)
            {
                query.Add(GetQueryForFilter<T>(filter));
            }

            @bool.Must(query.ToArray());

            return @bool;
        }

        private AggregationContainerDescriptor<T> GetFilterAggregations<T>(
            IEnumerable<string> aggs) where T : class
        {
            var returnVal = new AggregationContainerDescriptor<T>();

            foreach (var key in aggs)
            {
                returnVal.Terms(key, t => t
                    .Field(key)
                    .MinimumDocumentCount(2)
                    .Order(o => o.KeyAscending().CountDescending()));
            }

            return returnVal;
        }

        private QueryContainer GetQueryForFilter<T>(
            QueryFilter filter) where T : class
        {
            var comparatorHelpers = new ComparatorHelpers();

            var query =
                filter.Comparator == null
                    ? null
                    : comparatorHelpers.GetQuery<T>(
                          (dynamic)Comparator.All.FirstOrDefault(p => p.Value == filter?.Comparator?.Value), filter) ?? _queryBuilder.GetMatchQueryForFilter<T>(filter);

            return query;
        }

        private SearchDescriptor<TDocument> GetSearchDescriptor<TDocument>()
                                                            where TDocument : class, IDocument
        {
            return AggregatableFields.Any() ?
                BuildFilteredSearch<TDocument>(AggregatableFields) :
                BuildFilteredSearch<TDocument>();
        }

        private SortDescriptor<T> GetSortDescriptorForFilters<T>() where T : class
        {
            var sort = new SortDescriptor<T>();

            foreach (var filter in Filters)
            {
                if (filter.Sort?.IsSorted == true)
                {
                    var fieldSort = new FieldSortDescriptor<T>();
                    fieldSort.Field(filter.Field);
                    if (filter.Sort.IsSortedAsc == false)
                    {
                        fieldSort.Descending();
                    }
                    else
                    {   //default ascending
                        fieldSort.Ascending();
                    }
                    sort.Field(f => fieldSort);
                }
            }

            return sort;
        }
    }
}