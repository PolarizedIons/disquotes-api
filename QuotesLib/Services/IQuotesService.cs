using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuotesApi.Models.Paging;
using QuotesApi.Models.Quotes;

namespace QuotesLib.Services
{
    public interface IQuotesService
    {
        Task<Quote> FindById(Guid quoteId, bool onlyApproved = true, bool enrichWithUser = false);
        Task<Quote> FindByQuoteNumber(ulong guildId, int quoteNumber, bool onlyApproved = true, bool enrichWithUser = false);
        Task<PagedResponse<Quote>> FindApproved(IEnumerable<string> guildFilter, PagingFilter pagingFilter);
        Task<PagedResponse<Quote>> FindUnapproved(IEnumerable<string> guildFilter, PagingFilter pagingFilter);
        Task<Quote> ApproveQuote(Guid quoteId, Guid approverId);
        Task<Quote> AddQuote(QuoteDto quote, Guid userId);
        Task<bool> DeleteQuote(Guid quoteId);
    }
}
