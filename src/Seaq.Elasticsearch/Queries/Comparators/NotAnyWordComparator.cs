namespace Seaq.Elasticsearch.Queries.Comparators
{
    public class NotAnyWordComparator : Comparator
    {
        public NotAnyWordComparator()
            : base("!anyWord", "Does Not Match Any Word") { }
    }
}
