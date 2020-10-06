using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using QuotesApi.Models.Quotes;
using QuotesLib.Models;
using QuotesLib.Services;
using Serilog;

namespace QuotesBot.Services
{
    public class DiscordService : IDiscordService, ISingletonDiService
    {
        private IConfiguration _config;
        private DiscordSocketClient _client;

        public DiscordService(IConfiguration config)
        {
            _config = config;
            _client = new DiscordSocketClient();
        }

        public async Task LoginAndStart()
        {
            await _client.LoginAsync(TokenType.Bot, _config["Discord:BotToken"]);
            await _client.StartAsync();
        }

        public Task<bool> IsLoggedIn()
        {
            return Task.FromResult(_client.LoginState == LoginState.LoggedIn);
        }

        public Task<IEnumerable<IGuild>> GetMutualGuildsFor(ulong userId)
        {
            var user = _client.GetUser(userId);
            return Task.FromResult<IEnumerable<IGuild>>(user.MutualGuilds);
        }

        public Task<IGuild> GetGuild(ulong guildId)
        {
            return Task.FromResult<IGuild>(_client.GetGuild(guildId));
        }

        public Task<IUser> GetUser(ulong userId)
        {
            return Task.FromResult<IUser>(_client.GetUser(userId));
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
            var guild = await GetGuild(guildId) as SocketGuild;
            if (guild == null)
            {
                return false;
            }

            if (guild.OwnerId == userId)
            {
                return true;
            }

            var user = guild.GetUser(userId);
            return user.Roles.Any(role => role.Permissions.Administrator || role.Permissions.ManageGuild);
        }

        public async Task<ISelfUser> GetUserFromAuthToken(string token)
        {
            var myClient = new DiscordRestClient();
            await myClient.LoginAsync(TokenType.Bearer, token);
            return myClient.CurrentUser;
        }

        public async Task Logout()
        {
            await _client.LogoutAsync();
        }
    }
}
