using Nest;
using Seaq.Elasticsearch.Documents;
using Seaq.Elasticsearch.Extensions;
using Seaq.Elasticsearch.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seaq.Elasticsearch.Queries
{

    public class DefaultResultsBuilder : 
        IResultsBuilder
    {
        private readonly IFieldNameUtilities _fieldNameUtilities;
        private readonly IDocumentPropertyBuilder _documentPropertyBuilder;

        public DefaultResultsBuilder(
            IFieldNameUtilities fieldNameUtilities = null,
            IDocumentPropertyBuilder documentPropertyBuilder = null)
        {
            _fieldNameUtilities = fieldNameUtilities ?? new DefaultFieldNameUtilities();
            _documentPropertyBuilder = documentPropertyBuilder ?? new DefaultDocumentPropertyBuilder();
        }

        public IQueryResult GetQueryResultByCriteriaType<TDocument>(
            ICriteria<TDocument> criteria,
            ISearchResponse<IDocument> searchResults)
            where TDocument : class, IDocument
        {
            return criteria switch
            {
                SimpleQueryCriteria simpleQuery => GetSimpleQueryResult(simpleQuery, searchResults),
                SuggestionQueryCriteria suggestionQuery => GetSuggestionQueryResult(suggestionQuery, searchResults),
                FilteredQueryCriteria filteredQuery => GetFilteredQueryResult(filteredQuery, searchResults),
                GetByIdsQueryCriteria idQueryCriteria => GetByIdQueryResult(idQueryCriteria, searchResults),
                FieldValuesQueryCriteria fieldValuesCriteria => GetFieldValuesQueryResult(fieldValuesCriteria, searchResults),
                _ => GetBaseQueryResult(criteria, searchResults)
            };
        }

        private SimpleQueryResult GetSimpleQueryResult(
            SimpleQueryCriteria criteria,
            ISearchResponse<IDocument> searchResults)
        {
            var newPaging = new Paging(
                criteria.Paging.CurrentPage,
                criteria.Paging.PageSize,
                searchResults.Total);
            var newResultMeta = new DefaultResultMeta(searchResults);

            return new SimpleQueryResult(
                newPaging,
                searchResults.Documents.ToArray(),
                newResultMeta
            );
        }

        private SuggestionQueryResult GetSuggestionQueryResult(
            SuggestionQueryCriteria criteria,
            ISearchResponse<IDocument> searchResults)
        {
            var newPaging = new Paging(
                criteria.Paging.CurrentPage,
                criteria.Paging.PageSize,
                searchResults.Total);

            var newResultMeta = new DefaultResultMeta(searchResults);

            var buckets = searchResults.Aggregations.Any() ?
                new SuggestionBucket(
                    _fieldNameUtilities.GetElasticPropertyName(_documentPropertyBuilder.Type, nameof(IDocument.Type)),
                    searchResults.Aggregations?.Terms(SuggestionQueryCriteria.aggregateKey)?.Buckets.Select(x => x.BuildSuggestionBucket()).ToArray()) :
                    null
                ;

            return new SuggestionQueryResult(
                new SuggestionBucket[] { buckets },
                newPaging,
                searchResults.Documents.ToArray(),
                newResultMeta);
        }

        private FilteredQueryResult GetFilteredQueryResult(
            FilteredQueryCriteria criteria,
            ISearchResponse<IDocument> searchResults)
        {
            var newPaging = new Paging(
                criteria.Paging.CurrentPage,
                criteria.Paging.PageSize,
                searchResults.Total);

            var newResultMeta = new DefaultResultMeta(searchResults);

            var buckets = new List<SuggestionBucket>();

            foreach (var a in criteria.AggregatableFields)
            {
                var aggregations =
                    searchResults.Aggregations?.Terms(a)?.Buckets?.Where(x => x.Values.Any() || x.DocCount > 0).Select(x => x.BuildSuggestionBucket()).ToArray();
                if (aggregations != null)
                {
                    var bucket = new SuggestionBucket(a, aggregations);
                    buckets.Add(bucket);
                }
            }

            return new FilteredQueryResult(
                buckets.ToArray(),
                newPaging,
                searchResults.Documents.ToArray(),
                newResultMeta);
        }

        private FieldValuesQueryResult GetFieldValuesQueryResult(
            FieldValuesQueryCriteria criteria,
            ISearchResponse<IDocument> searchResults)
        {
            var newPaging = new Paging(
                criteria.Paging.CurrentPage,
                criteria.Paging.PageSize,
                searchResults.Total);

            var newResultMeta = new DefaultResultMeta(searchResults);

            var buckets = new List<SuggestionBucket>();

            foreach (var a in criteria.AggregatableFields)
            {
                var aggregations =
                    searchResults.Aggregations?
                        .Terms(a)?
                        .Buckets?
                        .Where(x => 
                            x.Values.Any() || 
                            x.DocCount > 0)
                        .Select(x => x.BuildSuggestionBucket())
                        .OrderByDescending(x => x.Count)
                        .ToArray();

                if (aggregations != null)
                {
                    var bucket = new SuggestionBucket(a, aggregations);
                    buckets.Add(bucket);
                }
            }

            return new FieldValuesQueryResult(
                buckets.ToArray(),
                newPaging,
                searchResults.Documents.ToArray(),
                newResultMeta);
        }

        private GetByIdsQueryResult GetByIdQueryResult(
            GetByIdsQueryCriteria criteria,
            ISearchResponse<IDocument> searchResults)
        {
            var newPaging = new Paging(
                criteria.Paging.CurrentPage,
                criteria.Paging.PageSize,
                searchResults.Total);
            
            var newResultMeta = new DefaultResultMeta(searchResults);

            return new GetByIdsQueryResult(
                newPaging,
                searchResults.Documents.ToArray(),
                newResultMeta
            );
        }

        private SimpleQueryResult GetBaseQueryResult<TDocument>(
            ICriteria<TDocument> criteria,
            ISearchResponse<IDocument> searchResults)
            where TDocument : class, IDocument
        {
            var newPaging = new Paging(
                criteria.Paging.CurrentPage,
                criteria.Paging.PageSize,
                searchResults.Total);

            var newResultMeta = new DefaultResultMeta(searchResults);

            return new SimpleQueryResult(
                newPaging,
                searchResults.Documents.ToArray(),
                newResultMeta
            );
        }

    }
}
