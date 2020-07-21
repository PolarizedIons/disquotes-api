using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuotesApi.Database;
using QuotesApi.Exceptions;
using QuotesApi.Models.Paging;
using QuotesApi.Models.Quotes;

namespace QuotesApi.Services
{
    public class QuoteService : IScopedDiService
    {
        private readonly DatabaseContext _db;
        private readonly DiscordService _discordService;
        private readonly UserService _userService;

        public QuoteService(DatabaseContext db, UserService userService, DiscordService discordService)
        {
            _db = db;
            _userService = userService;
            _discordService = discordService;
        }

        public Quote FindById(Guid quoteId, bool onlyApproved = true)
        {
            var query = _db.Quotes.AsQueryable().Where(x => x.Id == quoteId && x.DeletedAt == null);
            if (onlyApproved)
            {
                query = query.Where(x => x.Approved);
            }

            var quote = query.FirstOrDefault();

            if (quote == null)
            {
                throw new NotFoundException($"Quote with id '{quoteId}' not found.");
            }

            return quote;
        }

        public PagedResponse<Quote> FindApproved(IEnumerable<string> guildFilter, PagingFilter pagingFilter)
        {
            var query = _db.Quotes.AsQueryable()
                .Where(x => x.Approved)
                .Where(x => x.DeletedAt == null)
                .Where(x => guildFilter.Contains(x.GuildId));
                
            var data = query
                .Skip((pagingFilter.PageNumber - 1) * pagingFilter.PageSize)
                .Take(pagingFilter.PageSize);
            var totalCount = query.Count();
            
            return new PagedResponse<Quote>
            {
                Items = data,
                TotalRows = totalCount,
                HasNext = (pagingFilter.PageNumber * pagingFilter.PageSize) < totalCount,
                HasPrevious = pagingFilter.PageNumber > 1,
                PageNumber = pagingFilter.PageNumber,
                PageSize = pagingFilter.PageSize,
            };
        }

        public PagedResponse<Quote> FindUnapproved(IEnumerable<string> guildFilter, PagingFilter pagingFilter)
        {
            var query = _db.Quotes.AsQueryable()
                .Where(x => !x.Approved && x.DeletedAt == null)
                .Where(x => guildFilter.Contains(x.GuildId));
          
            var data = query
                .Skip((pagingFilter.PageNumber - 1) * pagingFilter.PageSize)
                .Take(pagingFilter.PageSize);
            var totalCount = query.Count();

            return new PagedResponse<Quote>
            {
                Items = data,
                TotalRows = totalCount,
                HasNext = (pagingFilter.PageNumber * pagingFilter.PageSize) < totalCount,
                HasPrevious = pagingFilter.PageNumber > 1,
                PageNumber = pagingFilter.PageNumber,
                PageSize = pagingFilter.PageSize,
            };
        }

        public async Task<Quote> ApproveQuote(Guid quoteId)
        {
            var quote = FindById(quoteId, false);

            var maxQuoteNumber = _db.Quotes.AsQueryable()
                                     .Where(x => x.Approved && x.DeletedAt == null)
                                     .OrderBy(x => x.QuoteNumber)
                                     .Select(x => x.QuoteNumber)
                                     .FirstOrDefault() ?? 0;

            quote.Approved = true;
            quote.QuoteNumber = maxQuoteNumber + 1;
            await _db.SaveChangesAsync();

            var user = await _userService.FindUser(quote.UserId);
            await _discordService.SendQuoteNotification(ulong.Parse(quote.GuildId), ulong.Parse(user.DiscordId), quote);

            return quote;
        }

        public async Task<Quote> Add(QuoteDto quote, Guid userId)
        {
            var newQuote = new Quote
            {
                Approved = false,
                Title = quote.Title,
                Text = quote.Text,
                GuildId = quote.GuildId,
                UserId = userId,
            };

            await _db.Quotes.AddAsync(newQuote);
            await _db.SaveChangesAsync();

            var user = await _userService.FindUser(userId);
            await _discordService.SendQuoteNotification(ulong.Parse(quote.GuildId), ulong.Parse(user.DiscordId), newQuote);
            
            return newQuote;
        }
    }
}
