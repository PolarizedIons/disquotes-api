using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Mapster;
using Microsoft.Extensions.Configuration;
using QuotesApi.Models.Quotes;
using QuotesLib.Models;
using QuotesLib.Models.Discord;
using QuotesLib.Services;
using Serilog;

namespace QuotesBot.Services
{
    public class DiscordService : IDiscordService, ISingletonDiService
    {
        private IConfiguration _config;
        private DiscordSocketClient _client;

        public DiscordService(IConfiguration config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
        }

        public async Task LoginAndStart()
        {
            await _client.LoginAsync(TokenType.Bot, _config["Discord:BotToken"]);
            await _client.StartAsync();
            
            _client.Log += log =>
            {
                if (log.Exception != null)
                {
                    Log.Debug(log.Exception, $"[Discord] {log.Source} {log.Message}");
                }
                else
                {
                    Log.Debug($"[Discord] {log.Source} {log.Message}");
                }

                return Task.CompletedTask;
            };

            _client.Ready += () =>
            {
                Log.Information("Discord bot ready!");
                return Task.CompletedTask;
            };

            _client.MessageReceived += message =>
            {
                if (message is SocketUserMessage msg)
                {
                    var guild = (msg.Author as SocketGuildUser)?.Guild;
                    Log.Debug($"[Message] [{msg.Author.Username}#{msg.Author.Discriminator} ({msg.Author.Id})] in ['{guild?.Name}'/#{msg.Channel.Name} ({guild?.Id}/{msg.Channel.Id})]: {msg.Content}");
                }

                return Task.CompletedTask;
            };
        }

        public Task<bool> IsLoggedIn()
        {
            return Task.FromResult(_client.LoginState == LoginState.LoggedIn);
        }

        public async Task<IEnumerable<MyIGuild>> GetMutualGuildsFor(ulong userId)
        {
            var user = _client.GetUser(userId);
            return user == null ? new MyIGuild[0] : user.MutualGuilds.Select(x => x.Adapt<MyIGuild>());
        }

        public Task<MyIGuild> GetGuild(ulong guildId)
        {
            return Task.FromResult(_client.GetGuild(guildId).Adapt<MyIGuild>());
        }

        public Task<MyIUser> GetUser(ulong userId)
        {
            return Task.FromResult(_client.GetUser(userId).Adapt<MyIUser>());
        }

        public async Task SendQuoteNotification(ulong guildId, Quote quote, ulong submitterId, ulong? approverId)
        {
            var guild = _client.GetGuild(guildId);

            if (guild.SystemChannel != null)
            {
                var user = guild.GetUser(submitterId);
                var approver = approverId.HasValue ? guild.GetUser(approverId.Value) : null;

                var text = quote.Approved ? _config["Notification:Approved"] : _config["Notification:New"];
                text = text
                    .Replace("{approver}", approver?.Mention)
                    .Replace("{submitter}", user.Mention)
                    .Replace("{title}", quote.Title);

                try
                {
                    Log.Debug("Sending {message} to {guild}/#{channel}", text, guild, guild.SystemChannel);
                    await guild.SystemChannel.SendMessageAsync(text);
                }
                catch (Discord.Net.HttpException e)
                {
                    Log.Debug("Could not send notification to guild id {guildId} ({channelId}): {exceptionMessage}", guildId, guild.SystemChannel.Id, e.Message);
                }
            }
        }

        public async Task<bool> IsModeratorInGuild(ulong userId, ulong guildId)
        {
            var guild = _client.GetGuild(guildId);
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

        public async Task<MyISelfUser> GetUserFromAuthToken(string token)
        {
            var myClient = new DiscordRestClient();
            await myClient.LoginAsync(TokenType.Bearer, token);
            return myClient.CurrentUser.Adapt<MyISelfUser>();
        }

        public async Task Logout()
        {
            await _client.LogoutAsync();
        }
    }
}
