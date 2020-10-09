using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuotesApi.Exceptions;
using QuotesApi.Models.Paging;
using QuotesApi.Models.Quotes;
using QuotesLib.Models;
using QuotesLib.Services;

namespace QuotesApi.Controllers
{
    [Route("quotes")]
    public class QuoteController : BaseController
    {
        private NatsQuotesService _natsQuoteService;
        private NatsDiscordService _natsDiscordService;

        public QuoteController(NatsQuotesService natsQuoteService, NatsDiscordService natsDiscordService)
        {
            _natsQuoteService = natsQuoteService;
            _natsDiscordService = natsDiscordService;
        }

        /// <summary>
        /// Gets approved quotes. Limited to quotes in guilds the authorized user is in.
        /// </summary>
        /// <param name="guildsFilter">(Optional) Comma separated list of guilds to retrieve.
        /// Defaults to, and is limited to, the authorized user's guilds</param>
        /// <param name="pagingFilter">Paging</param>
        [
            HttpGet,
            ProducesResponseType(typeof(ApiResult<PagedResponse<Quote>>), 200),
            Authorize
        ]
        public async Task<ApiResult<PagedResponse<Quote>>> GetApprovedQuotes([FromQuery] string? guildsFilter, [FromQuery] PagingFilter pagingFilter)
        {
            ValidatePagingFilter(pagingFilter);
            var userGuilds = (await _natsDiscordService.GetMutualGuildsFor(UserDiscordId)).Select(x => x.Id.ToString());
            var filter = guildsFilter?.Split(",").Where(x => userGuilds.Contains(x)) ?? userGuilds;
            return Ok(await _natsQuoteService.FindApproved(filter, pagingFilter));
        }

        /// <summary>
        /// Gets unapproved quotes. Limited to quotes in guilds the authorized user is admin in.
        /// </summary>
        /// <param name="guildsFilter">(Optional) Comma separated list of guilds to retrieve.
        /// Defaults to, and is limuted to, the authorized user's guilds where they are the owener</param>
        /// <param name="pagingFilter">Paging</param>
        [
            HttpGet("unmoderated"),
            ProducesResponseType(typeof(ApiResult<PagedResponse<Quote>>), 200),
            Authorize
        ]
        public async Task<ApiResult<PagedResponse<Quote>>> GetUnapprovedQuotes([FromQuery] string? guildsFilter, [FromQuery] PagingFilter pagingFilter)
        {
            ValidatePagingFilter(pagingFilter);
            var userGuilds = await _natsDiscordService.GetMutualGuildsFor(UserDiscordId);
            var filteredUserGuilds = new List<string>();

            foreach (var guild in userGuilds)
            {
                var isMod = await _natsDiscordService.IsModeratorInGuild(UserDiscordId, guild.Id);
                if (isMod)
                {
                    filteredUserGuilds.Add(guild.Id.ToString());
                }
            }

            var filter = guildsFilter?.Split(",").Where(x => filteredUserGuilds.Contains(x)) ?? filteredUserGuilds;
            return Ok(await _natsQuoteService.FindUnapproved(filter, pagingFilter));
        }

        /// <summary>
        /// Create a new quote. The authorized user AND the bot needs to be in the quild they are submitting to
        /// </summary>
        /// <param name="quote">The new quote</param>
        [
            HttpPost,
            ProducesResponseType(typeof(ApiResult<Quote>), 200),
            ProducesResponseType(typeof(ApiResult<object>), 403),
            Authorize,
        ]
        public async Task<ApiResult<Quote>> CreateQuote([FromBody] QuoteDto quote)
        {
            var userGuilds = (await _natsDiscordService.GetMutualGuildsFor(UserDiscordId)).Select(x => x.Id.ToString());
            if (!userGuilds.Contains(quote.GuildId))
            {
                throw new ForbiddenException("You cannot submit a quote for a guild you and the bot are not in.");
            }
            
            return Ok(await _natsQuoteService.AddQuote(quote, UserId));
        }

        /// <summary>
        /// Gets a quote by its id
        /// </summary>
        /// <param name="quoteId">The quote id</param>
        /// <returns></returns>
        [
            HttpGet("{quoteId:guid}"),
            ProducesResponseType(typeof(ApiResult<Quote>), 200),
            ProducesResponseType(typeof(ApiResult<object>), 404),
            Authorize,
        ]
        public async Task<ApiResult<Quote>> GetQuote([FromRoute] Guid quoteId)
        {
            var quote = await _natsQuoteService.FindById(quoteId, enrichWithUser: true);
            
            if (quote == null)
            {
                return NotFound("Quote not found.");
            }
            
            return Ok(quote);
        }
        
        /// <summary>
        /// Approve a quote. Requires the authorized user to be an owner of the guild the quote is in.
        /// </summary>
        /// <param name="quoteId">The quote id</param>
        [
            HttpPost("{quoteId:guid}/approve"),
            ProducesResponseType(typeof(ApiResult<Quote>), 200),
            ProducesResponseType(typeof(ApiResult<object>), 403),
            ProducesResponseType(typeof(ApiResult<object>), 404),
            Authorize,
        ]
        public async Task<ApiResult<Quote>> ApproveQuote([FromRoute] Guid quoteId)
        {
            var quote = await _natsQuoteService.FindById(quoteId, false);
            var isMod = await _natsDiscordService.IsModeratorInGuild(UserDiscordId, ulong.Parse(quote.GuildId));

            if (!isMod)
            {
                throw new ForbiddenException("Only the moderators of a guild can approve quotes submitted to it.");
            }

            return Ok(await _natsQuoteService.ApproveQuote(quoteId, UserId));
        }

        [
            HttpDelete("{quoteId:guid}"),
            ProducesResponseType(typeof(ApiResult<bool>), 200),
            Authorize
        ]
        public async Task<ApiResult<bool>> DeleteQuote([FromRoute] Guid quoteId)
        {
            var quote = await _natsQuoteService.FindById(quoteId, false);
            var isMod = await _natsDiscordService.IsModeratorInGuild(UserDiscordId, ulong.Parse(quote.GuildId));

            if (!isMod)
            {
                throw new ForbiddenException("Only the moderators of a guild can delete quotes submitted to it.");
            }

            return Ok(await _natsQuoteService.DeleteQuote(quoteId));
        }
    }
}
