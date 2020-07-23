using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuotesApi.Models;
using QuotesApi.Models.Guilds;
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
            ProducesResponseType(typeof(ApiResult<IEnumerable<Guild>>), 200),
            Authorize,
        ]
        public async Task<ApiResult<IEnumerable<Guild>>> GetGuilds()
        {

            var mutualGuilds = await _discordService.GetGuildsFor(UserDiscordId);
            var guilds = mutualGuilds
                .Select(x => new Guild
                {
                    Id = x.Id.ToString(),
                    Description = x.Description,
                    Name = x.Name,
                    IsOwner = UserDiscordId == x.OwnerId,
                    SystemChannelId = x.SystemChannelId.ToString(),
                    IconUrl = x.IconUrl,
                });

            return Ok(guilds);
        }

        [
            HttpGet("{guildId:ulong}"),
            ProducesResponseType(typeof(ApiResult<Guild>), 200),
            Authorize,
        ]
        public async Task<ApiResult<Guild>> GetGuildById([FromRoute] ulong guildId)
        {
            var discordGuild = await _discordService.GetGuild(guildId);
            var guild = new Guild
            {
                Description = discordGuild.Description,
                Id = discordGuild.Id.ToString(),
                Name = discordGuild.Name,
                IconUrl = discordGuild.IconUrl,
                IsOwner = UserDiscordId == discordGuild.OwnerId,
                SystemChannelId = discordGuild.SystemChannelId.ToString(),
            };

            return Ok(guild);
        }
    }
}
