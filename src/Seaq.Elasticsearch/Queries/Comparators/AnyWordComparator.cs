namespace Seaq.Elasticsearch.Queries.Comparators
{
    public class AnyWordComparator : Comparator
    {
        public AnyWordComparator()
            : base("anyWord", "Match Any Word") { }
    }
}
