using Seaq.Elasticsearch.Queries.Comparators;

namespace Seaq.Elasticsearch.Queries
{
    public class QueryFilter
    {
        public string Field { get; }
        public string Value { get; }
        public Comparator Comparator { get; }
        public Sort Sort { get; }

        public QueryFilter(
            string field,
            string value,
            Comparator comparator,
            Sort sort = null)
        {
            Field = field;
            Value = value;
            Comparator = comparator;
            Sort = sort ?? new Sort(false, false);
        }
    }
}
