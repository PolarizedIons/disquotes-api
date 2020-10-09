using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuotesApi.Models.Paging;
using QuotesApi.Models.Quotes;
using QuotesLib.Extentions;
using QuotesLib.Models;
using QuotesLib.Nats.Quotes;

namespace QuotesLib.Services
{
    public class NatsQuotesService : IQuotesService, ISingletonDiService
    {
        private readonly NatsService _natsService;

        public NatsQuotesService(NatsService natsService)
        {
            _natsService = natsService;
        }

        public async Task<Quote> FindById(Guid quoteId, bool onlyApproved = true, bool enrichWithUser = false)
        {
            var data = new FindByIdRequest
            {
                QuoteId = quoteId,
                OnlyApproved = onlyApproved,
                EnrichWithUser = enrichWithUser
            };
            var msg = await _natsService.RequestAsync(data);
            return msg.GetData<Quote>();
        }

        public async Task<PagedResponse<Quote>> FindApproved(IEnumerable<string> guildFilter, PagingFilter pagingFilter)
        {
            var data = new FindApprovedRequest
            {
                GuildFilter = guildFilter,
                PagingFilter = pagingFilter
            };
            var msg = await _natsService.RequestAsync(data);
            return msg.GetData<PagedResponse<Quote>>();
        }

        public async Task<PagedResponse<Quote>> FindUnapproved(IEnumerable<string> guildFilter, PagingFilter pagingFilter)
        {
            var data = new FindUnapprovedRequest
            {
                GuildFilter = guildFilter,
                PagingFilter = pagingFilter
            };
            var msg = await _natsService.RequestAsync(data);
            return msg.GetData<PagedResponse<Quote>>();
        }

        public async Task<Quote> ApproveQuote(Guid quoteId, Guid approverId)
        {
            var msg = await _natsService.RequestAsync(new ApproveQuoteRequest { QuoteId = quoteId, ApproverId = approverId});
            return msg.GetData<Quote>();
        }

        public async Task<Quote> AddQuote(QuoteDto quote, Guid userId)
        {
            var msg = await _natsService.RequestAsync(new AddQuoteRequest { Quote = quote, UserId = userId});
            return msg.GetData<Quote>();
        }

        public async Task<bool> DeleteQuote(Guid quoteId)
        {
            var msg = await _natsService.RequestAsync(new DeleteQuoteRequest { QuoteId = quoteId});
            return msg.GetData<bool>();
        }
    }
}