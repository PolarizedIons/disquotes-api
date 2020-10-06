using System.Collections.Generic;
using QuotesApi.Models.Paging;

namespace QuotesLib.Nats.Quotes
{
    public class FindApprovedRequest : INatsRequest
    {
        public IEnumerable<string> GuildFilter { get; set; } 
        public PagingFilter PagingFilter { get; set; }
    }
}