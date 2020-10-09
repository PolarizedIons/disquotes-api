using System.Collections.Generic;
using System.Threading.Tasks;
using QuotesBot.Services;
using QuotesLib.Models;
using QuotesLib.Models.Discord;
using QuotesLib.Nats;
using QuotesLib.Nats.Discord;
using QuotesLib.Services;

namespace QuotesBot.NatsResponders
{
    public class DiscordResponder : NatsResponder, ISingletonDiService
    {
        private readonly DiscordService _discordService;

        public DiscordResponder(DiscordService discordService, NatsService natsService) : base(natsService)
        {
            _discordService = discordService;
        }

        public Task<bool> OnIsLoggedIn(IsLoggedInRequest req)
        {
            return _discordService.IsLoggedIn();
        }

        public Task<IEnumerable<MyIGuild>> OnGetMutualGuildsFor(GetMutualGuildsRequest req)
        {
            return _discordService.GetMutualGuildsFor(req.DiscordId);
        }

        public Task<MyIGuild> OnGetGuild(GetGuildRequest req)
        {
            return _discordService.GetGuild(req.GuildId);
        }

        public Task<MyIUser> OnGetUser(GetUserRequest req)
        {
            return _discordService.GetUser(req.DiscordId);
        }

        public async Task OnSendQuoteNotification(SendQuoteNotificationRequest req)
        {
            await _discordService.SendQuoteNotification(req.GuildId, req.Quote, req.SubmitterId, req.ApproverId);
        }

        public Task<bool> OnIsModeratorInGuild(IsModeratorInGuildRequest req)
        {
            return _discordService.IsModeratorInGuild(req.UserId, req.GuildId);
        }

        public Task<MyISelfUser> OnGetUserFromAuthToken(GetUserFromAuthTokenRequest req)
        {
            return _discordService.GetUserFromAuthToken(req.Token);
        }
    }
}
