using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using QuotesApi.Exceptions;
using QuotesApi.Models.Paging;
using QuotesApi.Models.Quotes;
using QuotesApi.Models.Users;
using QuotesCore.Database;
using QuotesLib.Models;
using QuotesLib.Services;

namespace QuotesCore.Services
{
    public class QuoteService : IQuotesService, IScopedDiService
    {
        private readonly DatabaseContext _db;
        private readonly NatsDiscordService _natsDiscordService;
        private readonly UserService _userService;

        public QuoteService(DatabaseContext db, UserService userService, NatsDiscordService natsDiscordService)
        {
            _db = db;
            _userService = userService;
            _natsDiscordService = natsDiscordService;
        }

        public Task<Quote> FindById(Guid quoteId, bool onlyApproved = true, bool enrichWithUser = false)
        {
            var query = _db.Quotes.AsQueryable().Where(x => x.Id == quoteId && x.DeletedAt == null);
            if (onlyApproved)
            {
                query = query.Where(x => x.Approved);
            }

            if (enrichWithUser)
            {
                query = query
                    .Join(_db.Users, 
                        q => q.UserId, 
                        u => u.Id, 
                        (q, u) => Quote.Merge(q, u.Adapt<UserDto>())
                    );
            }

            return Task.FromResult(query.FirstOrDefault());
        }

        public Task<Quote> FindByQuoteNumber(ulong guildId, int quoteNumber, bool onlyApproved = true, bool enrichWithUser = false)
        {
            var query = _db.Quotes.AsQueryable()
                .Where(x => x.QuoteNumber == quoteNumber &&
                            x.GuildId == guildId.ToString() &&
                            x.DeletedAt == null
                );

            if (onlyApproved)
            {
                query = query.Where(x => x.Approved);
            }

            if (enrichWithUser)
            {
                query = query
                    .Join(_db.Users, 
                        q => q.UserId, 
                        u => u.Id, 
                        (q, u) => Quote.Merge(q, u.Adapt<UserDto>())
                    );
            }

            return Task.FromResult(query.FirstOrDefault());
        }

        public Task<PagedResponse<Quote>> FindApproved(IEnumerable<string> guildFilter, PagingFilter pagingFilter)
        {
            var query = _db.Quotes.AsQueryable()
                .Where(x => x.Approved)
                .Where(x => x.DeletedAt == null)
                .Where(x => guildFilter.Contains(x.GuildId))
                .OrderByDescending(x => x.LastModifiedAt)
                .ThenByDescending(x => x.QuoteNumber);
                
            var data = query
                .Skip((pagingFilter.PageNumber - 1) * pagingFilter.PageSize)
                .Take(pagingFilter.PageSize)
                .Join(_db.Users,
                    q => q.UserId,
                    u => u.Id,
                    (q, u) => Quote.Merge(q, u.Adapt<UserDto>())
                );
            var totalCount = query.Count();

            return Task.FromResult(new PagedResponse<Quote>
            {
                Items = data.ToList(),
                TotalRows = totalCount,
                HasNext = (pagingFilter.PageNumber * pagingFilter.PageSize) < totalCount,
                HasPrevious = pagingFilter.PageNumber > 1,
                PageNumber = pagingFilter.PageNumber,
                PageSize = pagingFilter.PageSize,
            });
        }

        public Task<PagedResponse<Quote>> FindUnapproved(IEnumerable<string> guildFilter, PagingFilter pagingFilter)
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
                    (q, u) => Quote.Merge(q, u.Adapt<UserDto>())
                );
            var totalCount = query.Count();

            return Task.FromResult(new PagedResponse<Quote>
            {
                Items = data.ToList(),
                TotalRows = totalCount,
                HasNext = (pagingFilter.PageNumber * pagingFilter.PageSize) < totalCount,
                HasPrevious = pagingFilter.PageNumber > 1,
                PageNumber = pagingFilter.PageNumber,
                PageSize = pagingFilter.PageSize,
            });
        }

        public async Task<Quote> ApproveQuote(Guid quoteId, Guid approverId)
        {
            var quote = await FindById(quoteId, false);

            if (quote.Approved)
            {
                throw new BadRequestException("Cannot approve quote that is already approved.");
            }

            var maxQuoteNumber = _db.Quotes.AsQueryable()
                                     .Where(x => 
                                         x.Approved &&
                                         x.GuildId == quote.GuildId &&
                                         x.DeletedAt == null
                                        )
                                     .OrderByDescending(x => x.QuoteNumber)
                                     .Select(x => x.QuoteNumber)
                                     .FirstOrDefault() ?? 0;

            quote.Approved = true;
            quote.QuoteNumber = maxQuoteNumber + 1;
            await _db.SaveChangesAsync();

            var user = await _userService.FindUser(quote.UserId);
            var approver = await _userService.FindUser(approverId);
            await _natsDiscordService.SendQuoteNotification(ulong.Parse(quote.GuildId), quote, ulong.Parse(user.DiscordId), ulong.Parse(approver.DiscordId));

            return quote;
        }

        public async Task<Quote> AddQuote(QuoteDto quote, Guid userId)
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
            newQuote.User = user.Adapt<UserDto>();
            await _natsDiscordService.SendQuoteNotification(ulong.Parse(quote.GuildId), newQuote, ulong.Parse(user.DiscordId), null);
            
            return newQuote;
        }

        public async Task<bool> DeleteQuote(Guid quoteId)
        {
            var quote = await FindById(quoteId, false);
            quote.DeletedAt = DateTime.UtcNow;
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }
    }
}
