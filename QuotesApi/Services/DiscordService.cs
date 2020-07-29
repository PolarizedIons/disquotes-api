using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Microsoft.Extensions.Configuration;
using QuotesApi.Exceptions;
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

        public bool IsLoggedIn => _client.LoginState == LoginState.LoggedIn;

        public async Task<List<RestGuild>> GetMutualGuildsFor(ulong userId)
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
            try
            {
                return await _client.GetGuildAsync(guildId);
            }
            catch (Discord.Net.HttpException)
            {
                throw new NotFoundException($"Discord server '{guildId}' not found, make sure the bot has access!");
            }
        }

        public async Task<RestUser> GetUser(ulong userId)
        {
            try
            {
                return await _client.GetUserAsync(userId);
            }
            catch (Discord.Net.HttpException)
            {
                throw new NotFoundException($"Discord user '{userId}' not found, make sure the bot shares a server!");
            }
        }

        public async Task SendQuoteNotification(ulong guildId, Quote quote, ulong submitterId, ulong? approverId)
        {
            var guild = await GetGuild(guildId);
            var channel = await guild.GetSystemChannelAsync();

            if (channel != null)
            {
                var user = await guild.GetUserAsync(submitterId);
                var approver = approverId.HasValue ? await guild.GetUserAsync(approverId.Value) : null;

                var text = quote.Approved ? _config["Notification:Approved"] : _config["Notification:New"];
                text = text
                    .Replace("{approver}", approver?.Mention)
                    .Replace("{submitter}", user.Mention)
                    .Replace("{title}", quote.Title);

                try
                {
                    await channel.SendMessageAsync(text);
                }
                catch (Discord.Net.HttpException e)
                {
                    Log.Debug("Could not send notification to guild id {guildId} ({channelId}): {exceptionMessage}", guildId, channel.Id, e.Message);
                }
            }
        }

        public async Task<bool> IsModeratorInGuild(ulong userId, ulong guildId)
        {
            var guild = await GetGuild(guildId);

            if (guild.OwnerId == userId)
            {
                return true;
            }

            var user = await guild.GetUserAsync(userId);

            var roles = user.RoleIds
                .Select(userRole => guild.Roles.First(x => x.Id == userRole));

            return roles.Any(role => role.Permissions.Administrator || role.Permissions.ManageGuild);
        }
    }
}
