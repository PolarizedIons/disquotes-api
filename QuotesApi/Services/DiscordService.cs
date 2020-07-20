using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Microsoft.Extensions.Configuration;
using QuotesApi.Models.Quotes;
using Serilog;

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

        public async Task<RestGuild> GetGuild(ulong guildId)
        {
            return await _client.GetGuildAsync(guildId);
        }

        public async Task SendQuoteNotification(ulong guildId, ulong discordUserId, Quote quote)
        {
            var guild = await GetGuild(guildId);
            var channel = await guild.GetSystemChannelAsync();

            if (channel != null)
            {
                var user = await guild.GetUserAsync(discordUserId);
                var owner = await guild.GetOwnerAsync();

                var text = quote.Approved ? _config["Notification:Approved"] : _config["Notification:New"];
                text = text
                    .Replace("{owner}", owner.Mention)
                    .Replace("{submitter}", user.Mention)
                    .Replace("{title}", quote.Title);

                try
                {
                    await channel.SendMessageAsync(text);
                }
                catch (Discord.Net.HttpException e)
                {
                    Log.Debug("Could not send notification to guild id {guildId}, {exceptionMessage}", guildId, e.Message);
                }
            }
        }
    }
}
