namespace Seaq.Elasticsearch.Queries
{
    public interface IPaging
    {
        int CurrentPage { get; }
        int PageSize { get; }
        int? TotalPages { get; }
        long? TotalItems { get; }
    }
}
