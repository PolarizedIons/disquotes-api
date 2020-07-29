using System.Collections.Generic;
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

            var mutualGuilds = await _discordService.GetMutualGuildsFor(UserDiscordId);
            var guilds = new List<Guild>();

            foreach (var guild in mutualGuilds)
            {
                var isMod = await _discordService.IsModeratorInGuild(UserDiscordId, guild.Id);

                guilds.Add(new Guild
                {
                    Id = guild.Id.ToString(),
                    Description = guild.Description,
                    Name = guild.Name,
                    IsModerator = isMod,
                    SystemChannelId = guild.SystemChannelId.ToString(),
                    IconUrl = guild.IconUrl,
                });
            }

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
                IsModerator = await _discordService.IsModeratorInGuild(UserDiscordId, guildId),
                SystemChannelId = discordGuild.SystemChannelId.ToString(),
            };

            return Ok(guild);
        }
    }
}
