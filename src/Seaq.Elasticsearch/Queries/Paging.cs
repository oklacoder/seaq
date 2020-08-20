using System;

namespace Seaq.Elasticsearch.Queries
{

    public class Paging :
        IPaging
    {
        public int CurrentPage { get; }

        public int PageSize { get; }

        public int? TotalPages
        {
            get
            {
                return TotalItems.HasValue ? (int)Math.Ceiling((double)(TotalItems / PageSize)) : (int?)null;
            }
        }

        public long? TotalItems { get; }

        public Paging()
        {
            CurrentPage = 1;
            PageSize = 50;
        }

        public Paging(
            int currentPage,
            int pageSize,
            long totalItems)
        {
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalItems = totalItems;
        }
    }
}
