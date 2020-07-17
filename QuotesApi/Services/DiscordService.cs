using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Microsoft.Extensions.Configuration;

namespace QuotesApi.Services
{
    public class DiscordService : ISingletonDiService
    {
        private IConfiguration _config;
        private DiscordRestClient _client;

        public DiscordService(IConfiguration config)
        {
            _config = config;
            _client = new DiscordRestClient();
        }

        public async Task Login()
        {
            await _client.LoginAsync(TokenType.Bot, _config["Discord:BotToken"]);
        }

        public async Task<List<RestGuild>> GetGuildsFor(ulong userId)
        {
            var userGuilds = new List<RestGuild>();
            var botGuilds = await _client.GetGuildsAsync();
            foreach (var guild in botGuilds)
            {
                var guildUser = await guild.GetUserAsync(userId);
                if (guildUser != null)
                {
                    userGuilds.Add(guild);
                }
            }

            return userGuilds;
        }
    }
}
