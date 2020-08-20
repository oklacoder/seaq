using Nest;
using Seaq.Elasticsearch.Clusters;
using Seaq.Elasticsearch.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Seaq.Elasticsearch.Queries
{
    public class SuggestionQueryCriteria :
        ICriteria<ISkinnyDocument>
    {
        public static string aggregateKey = "types";
        private const int DefaultSuggestionQuerySize = 10;
        private readonly IFieldNameUtilities _fieldNameUtilities;
        private readonly IDocumentPropertyBuilder _propertyBuilder;
        private readonly IQueryBuilder _queryBuilder;
        
        public int Size { get; }
        public string Text { get; }
        public Paging _paging { get; }
        public IPaging Paging => _paging;
        public ImmutableList<string> StoreIdNames { get; }

        public SuggestionQueryCriteria(
            IEnumerable<string> storeIdNames,
            string text,
            Paging paging = null,
            int? size = DefaultSuggestionQuerySize,
            IDocumentPropertyBuilder propertyBuilder = null,
            IFieldNameUtilities fieldNameUtilities = null,
            IQueryBuilder queryBuilder = null)
        {
            StoreIdNames = ImmutableList<string>.Empty.AddRange(storeIdNames);

            Text = text;
            _paging = paging ?? new Paging();
            Size = size ?? DefaultSuggestionQuerySize;
            _propertyBuilder = propertyBuilder ?? new DefaultDocumentPropertyBuilder();
            _fieldNameUtilities = fieldNameUtilities ?? new DefaultFieldNameUtilities();
            _queryBuilder = queryBuilder ?? new DefaultQueryBuilder();
        }
               
        public Func<SearchDescriptor<ISkinnyDocument>, ISearchRequest> GetDescriptor()
        {
            var returnValue =
                new Func<SearchDescriptor<ISkinnyDocument>, ISearchRequest>(
                    descriptor => GetSearchDescriptor());
            return returnValue;
        }
        
        private SearchDescriptor<ISkinnyDocument> GetSearchDescriptor()
        {
            var type = typeof(ISkinnyDocument);

            var fields =
                _propertyBuilder
                    .GetPropertyNames()
                    .Select(x =>
                        _fieldNameUtilities.GetElasticPropertyNameWithoutSuffix(type, x))
                    .ToArray();

            var search = new SearchDescriptor<ISkinnyDocument>()
                .Index(Indices.Index(StoreIdNames))
                .Source(sf => _queryBuilder.BuildSourceFilter<ISkinnyDocument>(fields))
                .Query(q => BuildSuggestionQuery<ISkinnyDocument>(Text))
                .Size(Size)
                .Aggregations(agg => BuildSuggestionAggregation<ISkinnyDocument>(aggregateKey));

            return search;
        }

        private AggregationContainerDescriptor<T> BuildSuggestionAggregation<T>(
            string aggKey)
            where T : class
        {
            var agg = new AggregationContainerDescriptor<T>();

            agg.Terms(aggKey, t => t
                .Field(_fieldNameUtilities.GetElasticPropertyName(typeof(T), nameof(IDocument.Type)))
            );

            return agg;
        }

        private QueryContainerDescriptor<T> BuildSuggestionQuery<T>(
            string query) where T : class
        {
            var search = new QueryContainerDescriptor<T>();

            search
                .Bool(b => b
                    .Filter(f => f
                        .Bool(fs => fs
                            .Must(m => m
                                .MatchPhrasePrefix(mp => mp
                                    .Field(_fieldNameUtilities.GetElasticPropertyNameWithoutSuffix(typeof(T), nameof(IDocument.Suggestions)))
                                    .Query(query)
                                )
                            )
                        )
                    )
                );

            return search;
        }
        
        public void CollectMetadataForQuery(Cluster cluster)
        {

        }
    }
}