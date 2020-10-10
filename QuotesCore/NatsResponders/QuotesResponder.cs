using System.Threading.Tasks;
using QuotesApi.Models.Paging;
using QuotesApi.Models.Quotes;
using QuotesCore.Services;
using QuotesLib.Models;
using QuotesLib.Nats;
using QuotesLib.Nats.Quotes;
using QuotesLib.Services;

namespace QuotesCore.NatsResponders
{
    public class QuotesResponder : NatsResponder, ISingletonDiService
    {
        private readonly QuoteService _quoteService;

        public QuotesResponder(QuoteService quoteService, NatsService natsService) : base(natsService)
        {
            _quoteService = quoteService;
        }

        public Task<Quote> OnFindById(FindByIdRequest req)
        {
            return _quoteService.FindById(req.QuoteId, req.OnlyApproved, req.EnrichWithUser);
        }
        
        public Task<Quote> OnFindByQuoteNumber(FindByQuoteNumberRequest req)
        {
            return _quoteService.FindByQuoteNumber(req.GuildId, req.QuoteNumber, req.OnlyApproved, req.EnrichWithUser);
        }

        public Task<PagedResponse<Quote>> OnFindApproved(FindApprovedRequest req)
        {
            return _quoteService.FindApproved(req.GuildFilter, req.PagingFilter);
        }
        

        public Task<PagedResponse<Quote>> OnFindUnapproved(FindUnapprovedRequest req)
        {
            return _quoteService.FindUnapproved(req.GuildFilter, req.PagingFilter);
        }

        public Task<Quote> OnApproveQuote(ApproveQuoteRequest req)
        {
            return _quoteService.ApproveQuote(req.QuoteId, req.ApproverId);
        }

        public Task<Quote> OnAddQuote(AddQuoteRequest req)
        {
            return _quoteService.AddQuote(req.Quote, req.UserId);
        }

        public Task<bool> OnDeleteQuote(DeleteQuoteRequest req)
        {
            return _quoteService.DeleteQuote(req.QuoteId);
        }
    }
}
