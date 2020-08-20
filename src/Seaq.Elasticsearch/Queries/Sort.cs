namespace Seaq.Elasticsearch.Queries
{
    public class Sort
    {
        public bool IsSorted { get; }
        public bool IsSortedAsc { get; }

        public Sort(
            bool isSorted,
            bool isSortedAsc)
        {
            IsSorted = isSorted;
            IsSortedAsc = isSortedAsc;
        }
    }
}
