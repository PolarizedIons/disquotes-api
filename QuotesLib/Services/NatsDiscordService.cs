using System.Collections.Generic;
using System.Threading.Tasks;
using QuotesApi.Models.Quotes;
using QuotesLib.Extentions;
using QuotesLib.Models;
using QuotesLib.Models.Discord;
using QuotesLib.Nats.Discord;

namespace QuotesLib.Services
{
    public class NatsDiscordService : IDiscordService, ISingletonDiService
    {
        private readonly NatsService _natsService;

        public NatsDiscordService(NatsService natsService)
        {
            _natsService = natsService;
        }
        
        public async Task<bool> IsLoggedIn()
        {
            var msg = await _natsService.RequestAsync(new IsLoggedInRequest());
            return msg.GetData<bool>();
        }

        public async Task<IEnumerable<MyIGuild>> GetMutualGuildsFor(ulong userId)
        {
            var msg = await _natsService.RequestAsync(new GetMutualGuildsRequest{ DiscordId = userId });
            return msg.GetData<IEnumerable<MyIGuild>>();
        }

        public async Task<MyIGuild> GetGuild(ulong guildId)
        {
            var msg = await _natsService.RequestAsync(new GetGuildRequest{ GuildId = guildId });
            return msg.GetData<MyIGuild>();
        }

        public async Task<MyIUser> GetUser(ulong userId)
        {
            var msg = await _natsService.RequestAsync(new GetUserRequest{ DiscordId = userId });
            return msg.GetData<MyIUser>();
        }

        public async Task SendQuoteNotification(ulong guildId, Quote quote, ulong submitterId, ulong? approverId)
        {
            var data = new SendQuoteNotificationRequest
            {
                GuildId = guildId,
                Quote = quote,
                SubmitterId = submitterId,
                ApproverId = approverId,
            };
            await _natsService.RequestAsync(data);
        }

        public async Task<bool> IsModeratorInGuild(ulong userId, ulong guildId)
        {
            var msg = await _natsService.RequestAsync(new IsModeratorInGuildRequest{ UserId = userId, GuildId = guildId });
            return msg.GetData<bool>();
        }

        public async Task<MyISelfUser> GetUserFromAuthToken(string token)
        {
            var msg = await _natsService.RequestAsync(new GetUserFromAuthTokenRequest { Token = token });
            return msg.GetData<MyISelfUser>();
        }
    }
}
