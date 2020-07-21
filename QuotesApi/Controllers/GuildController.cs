using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuotesApi.Models;
using QuotesApi.Models.Guilds;
using QuotesApi.Models.Paging;
using QuotesApi.Services;

namespace QuotesApi.Controllers
{
    [Route("guilds")]
    public class GuildController : BaseController
    {
        private readonly DiscordService _discordService;

        public GuildController(DiscordService discordService)
        {
            _discordService = discordService;
        }
        
        [
            HttpGet,
            ProducesResponseType(typeof(ApiResult<PagedResponse<Guild>>), 200),
            Authorize,
        ]
        public async Task<ApiResult<PagedResponse<Guild>>> GetGuilds([FromQuery] PagingFilter pagingFilter)
        {
            ValidatePagingFilter(pagingFilter);

            var mutualGuilds = await _discordService.GetGuildsFor(UserDiscordId);
            var guilds = mutualGuilds
                .Skip((pagingFilter.PageNumber - 1) * pagingFilter.PageSize)
                .Take(pagingFilter.PageSize)
                .Select(x => new Guild
                {
                    Id = x.Id.ToString(),
                    Description = x.Description,
                    Name = x.Name,
                    IsOwner = UserDiscordId == x.OwnerId,
                    SystemChannelId = x.SystemChannelId.ToString(),
                    IconUrl = x.IconUrl,
                });

            var data = new PagedResponse<Guild>
            {
                Items = guilds,
                HasNext = (pagingFilter.PageNumber * pagingFilter.PageSize) < mutualGuilds.Count,
                HasPrevious = pagingFilter.PageNumber > 1,
                PageNumber = pagingFilter.PageNumber,
                PageSize = pagingFilter.PageSize,
                TotalRows = mutualGuilds.Count,
            };
            
            return Ok(data);
        }
    }
}