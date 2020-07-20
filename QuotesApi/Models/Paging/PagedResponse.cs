using System.Collections.Generic;

namespace QuotesApi.Models.Paging
{
    public class PagedResponse<T> : PagingFilter
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalRows { get; set; }
        public bool HasNext  { get; set; }
        public bool HasPrevious { get; set; }
    }
}
