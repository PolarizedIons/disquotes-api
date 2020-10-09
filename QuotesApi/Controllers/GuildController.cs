using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuotesApi.Models.Guilds;
using QuotesLib.Models;
using QuotesLib.Services;

namespace QuotesApi.Controllers
{
    [Route("guilds")]
    public class GuildController : BaseController
    {
        private readonly NatsDiscordService _natsDiscordService;

        public GuildController(NatsDiscordService natsDiscordService)
        {
            _natsDiscordService = natsDiscordService;
        }

        [
            HttpGet,
            ProducesResponseType(typeof(ApiResult<IEnumerable<Guild>>), 200),
            Authorize,
        ]
        public async Task<ApiResult<IEnumerable<Guild>>> GetGuilds()
        {

            var mutualGuilds = await _natsDiscordService.GetMutualGuildsFor(UserDiscordId);
            var guilds = new List<Guild>();

            foreach (var guild in mutualGuilds)
            {
                var isMod = await _natsDiscordService.IsModeratorInGuild(UserDiscordId, guild.Id);

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
            var discordGuild = await _natsDiscordService.GetGuild(guildId);
            if (discordGuild == null)
            {
                return NotFound("The bot cannot access that guild.");
            }

            var guild = new Guild
            {
                Description = discordGuild.Description,
                Id = discordGuild.Id.ToString(),
                Name = discordGuild.Name,
                IconUrl = discordGuild.IconUrl,
                IsModerator = await _natsDiscordService.IsModeratorInGuild(UserDiscordId, guildId),
                SystemChannelId = discordGuild.SystemChannelId.ToString(),
            };

            return Ok(guild);
        }
    }
}
