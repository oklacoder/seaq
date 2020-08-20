namespace Seaq.Elasticsearch.Queries.Comparators
{
    public class NotEqualComparator : Comparator
    {
        public NotEqualComparator() :
            base("!=", "Not Equal")
        { }
    }
}
