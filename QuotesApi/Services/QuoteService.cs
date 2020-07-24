using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuotesApi.Database;
using QuotesApi.Exceptions;
using QuotesApi.Extentions;
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

        public Quote FindById(Guid quoteId, bool onlyApproved = true, bool enrichWithUser = false)
        {
            var query = _db.Quotes.AsQueryable().Where(x => x.Id == quoteId && x.DeletedAt == null);
            if (onlyApproved)
            {
                query = query.Where(x => x.Approved);
            }

            if (enrichWithUser)
            {
                query = query
                    .AsNoTracking()
                    .Join(_db.Users, 
                        q => q.UserId, 
                        u => u.Id, 
                        (q, u) => new Quote().MapProps(q).MapProps(new {User = u})
                    );
            }

            var quote = query.FirstOrDefault();

            if (quote == null)
            {
                throw new NotFoundException($"Quote with id '{quoteId}' not found.");
            }

            if (enrichWithUser)
            {
                _db.Attach(quote);
            }

            return quote;
        }

        public PagedResponse<Quote> FindApproved(IEnumerable<string> guildFilter, PagingFilter pagingFilter)
        {
            var query = _db.Quotes.AsQueryable()
                .Where(x => x.Approved)
                .Where(x => x.DeletedAt == null)
                .Where(x => guildFilter.Contains(x.GuildId))
                .OrderByDescending(x => x.LastModifiedAt);
                
            var data = query
                .Skip((pagingFilter.PageNumber - 1) * pagingFilter.PageSize)
                .Take(pagingFilter.PageSize)
                .Join(_db.Users,
                    q => q.UserId,
                    u => u.Id,
                    (q, u) => new Quote().MapProps(q).MapProps(new {User = u})
                );
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
                .Where(x => guildFilter.Contains(x.GuildId))
                .OrderByDescending(x => x.LastModifiedAt);
          
            var data = query
                .Skip((pagingFilter.PageNumber - 1) * pagingFilter.PageSize)
                .Take(pagingFilter.PageSize)
                .Join(_db.Users, 
                    q => q.UserId, 
                    u => u.Id, 
                    (q, u) => new Quote().MapProps(q).MapProps(new {User = u})
                );
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

            if (quote.Approved)
            {
                throw new BadRequestException("Cannot approve quote that is already approved.");
            }

            var maxQuoteNumber = _db.Quotes.AsQueryable()
                                     .Where(x => x.Approved && x.DeletedAt == null)
                                     .OrderByDescending(x => x.QuoteNumber)
                                     .Select(x => x.QuoteNumber)
                                     .FirstOrDefault() ?? 0;

            quote.Approved = true;
            quote.QuoteNumber = maxQuoteNumber + 1;
            await _db.SaveChangesAsync();

            var user = _userService.FindUser(quote.UserId);
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

            var user = _userService.FindUser(userId);
            await _discordService.SendQuoteNotification(ulong.Parse(quote.GuildId), ulong.Parse(user.DiscordId), newQuote);
            
            return newQuote;
        }

        public async Task<bool> DeleteQuote(Guid quoteId)
        {
            var quote = FindById(quoteId, false);
            quote.DeletedAt = DateTime.UtcNow;
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }
    }
}
