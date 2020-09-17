using Nest;
using Seaq.Elasticsearch.Clusters;
using Seaq.Elasticsearch.Documents;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Seaq.Elasticsearch.Queries
{
    public class SimpleQueryCriteria :
        ICriteria<IDocument>
    {
        public SimpleQueryCriteria(
            IEnumerable<string> storeIdNames,
            string text = null,
            Paging paging = null)
        {
            StoreIdNames = ImmutableList<string>.Empty.AddRange(storeIdNames);

            Text = text;
            _paging = paging ?? new Paging();
        }


        public string[] AggregatableFields { get; set; }
        public string[] BoostableFields { get; set; }
        public string[] FieldsToInclude { get; set; }

        public string Text { get; }

        public Paging _paging { get; }
        public IPaging Paging => _paging;

        public ImmutableList<string> StoreIdNames { get; }

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
            var fieldString = string.Join(",", BoostableFields);

            return new SearchDescriptor<TDocument>()
                .Index(Indices.Index(StoreIdNames))
                .Source(s => BuildSourceFilter<TDocument>())
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            sh => sh
                                .MultiMatch(mm => mm
                                    .Operator(Operator.Or)
                                    .Type(TextQueryType.PhrasePrefix)
                                    .Query(Text)),
                            sh => sh
                                .MultiMatch(mm => mm
                                    .Query(Text)
                                    .Operator(Operator.Or)
                                    .Type(TextQueryType.BestFields)
                                    .Lenient()
                                    .Fields(fieldString)))));
        }
        private SourceFilterDescriptor<TDocument> BuildSourceFilter<TDocument>()
            where TDocument : class
        {
            var filter = new SourceFilterDescriptor<TDocument>();

            if (FieldsToInclude.Any(x =>
                !WellKnownKeys.Fields.ConstantReturnedFields.Any(y =>
                    y.Equals(x, StringComparison.OrdinalIgnoreCase))))
            {
                filter.Includes(x => x.Fields(FieldsToInclude));
            }
            else
            {
                filter.IncludeAll();
            }

            return filter;
        }

        public void CollectMetadataForQuery(Cluster cluster)
        {
            var schemas = cluster?.GetStoreSchemas(StoreIdNames.ToArray());

            BoostableFields = schemas?.SelectMany(x => x?.GetAllBoostedFieldNames())?.ToArray();
            FieldsToInclude = schemas?.SelectMany(x => x.GetFieldsToInclude())?.ToArray() ?? new string[] { };
        }
    }
}