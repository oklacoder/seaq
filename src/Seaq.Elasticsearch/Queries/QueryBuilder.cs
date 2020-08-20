using Nest;

namespace Seaq.Elasticsearch.Queries
{
    public class DefaultQueryBuilder : 
        IQueryBuilder
    {
        private readonly IFieldNameUtilities _fieldNameUtilities;
        private readonly IDocumentPropertyBuilder _propertyBuilder;

        public DefaultQueryBuilder(
            IDocumentPropertyBuilder propertyBuilder = null,
            IFieldNameUtilities fieldNameUtilities = null)
        {
            _propertyBuilder = propertyBuilder ?? new DefaultDocumentPropertyBuilder();
            _fieldNameUtilities = fieldNameUtilities ?? new DefaultFieldNameUtilities();
        }

        public QueryContainerDescriptor<T> GetMatchPhrasePrefixQueryForFilter<T>(
            QueryFilter filter) where T : class
        {
            var match = new QueryContainerDescriptor<T>();
            var isCSV = filter.Value.Contains(",");
            if (isCSV)
            {
                var vals = filter.Value.Split(',');
                foreach (var v in vals)
                {//fallback to match, as match phrase prefix doesn't support "or" queries
                    match.Match(p => p.Field(filter.Field).Query(v).Operator(Operator.Or));
                }
            }
            else
            {
                match.MatchPhrasePrefix(p => p.Field(filter.Field).Query(filter.Value));
            }

            return match;
        }

        public QueryContainerDescriptor<T> GetMatchPhraseQueryForFilter<T>(
            QueryFilter filter) where T : class
        {
            var match = new QueryContainerDescriptor<T>();
            var isCSV = filter.Value.Contains(",");
            if (isCSV)
            {
                var vals = filter.Value.Split(',');
                foreach (var v in vals)
                {//fallback to match, as match phrase doesn't support "or" queries
                    match.Match(p => p.Field(filter.Field).Query(v).Operator(Operator.Or));
                }
            }
            else
            {
                match.MatchPhrase(p => p.Field(filter.Field).Query(filter.Value));
            }

            return match;
        }

        public QueryContainerDescriptor<T> GetMatchQueryForFilter<T>(
            QueryFilter filter) where T : class
        {
            var match = new QueryContainerDescriptor<T>();

            var isCSV = filter.Value.Contains(",");
            //var isNested = filter.Field.Contains(".");
            var isNested = System.Text.RegularExpressions.Regex.IsMatch(filter.Field, @"^(?=.*\.)(?!.*\.keyword).*");
            if (isCSV)
            {
                var vals = filter.Value.Split(',');

                if (isNested)
                {
                    match.Match(ma => ma.Query(filter.Value).Field($"{filter.Field}"));
                }
                else
                {
                    //m.Match(p => p.Field(f.Field).Query(v).Operator(Operator.Or));
                    match.Terms(p => p.Field($"{filter.Field}").Terms(vals));
                }
            }
            else
            {
                if (isNested)
                {
                    match.Match(ma => ma.Query(filter.Value).Field($"{filter.Field}"));
                }
                else
                {
                    match.Match(p => p.Field(filter.Field).Query(filter.Value));
                }
            }

            return match;
        }


        public SourceFilterDescriptor<T> BuildSourceFilter<T>(
            string[] fields) where T : class
        {
            var sf = new SourceFilterDescriptor<T>();

            sf.Includes(inc => inc
                .Fields(fields)
            );

            return sf;
        }
    }
}
