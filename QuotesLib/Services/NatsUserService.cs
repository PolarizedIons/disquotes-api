using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuotesApi.Models.Users;
using QuotesLib.Extentions;
using QuotesLib.Models;
using QuotesLib.Models.Discord;
using QuotesLib.Models.Security;
using QuotesLib.Nats.Users;

namespace QuotesLib.Services
{
    public class NatsUserService : IUserService, ISingletonDiService
    {
        private readonly NatsService _natsService;

        public NatsUserService(NatsService natsService)
        {
            _natsService = natsService;
        }
        
        public async Task<IEnumerable<User>> FindAllUsers()
        {
            var msg = await _natsService.RequestAsync(new FindAllUsersRequest());
            return msg.GetData<IEnumerable<User>>();
        }

        public async Task<User> FindUser(Guid userId)
        {
            var msg = await _natsService.RequestAsync(new FindUserRequest { UserId = userId });
            return msg.GetData<User>();
        }

        public async Task<User> FindDiscordUser(ulong discordId)
        {
            var msg = await _natsService.RequestAsync(new FindDiscordUserRequest { UserId = discordId });
            return msg.GetData<User>();
        }

        public async Task<User> LoginDiscordUser(MyIUser discordUser)
        {
            var msg = await _natsService.RequestAsync(new LoginDiscordUserRequest { User = discordUser });
            return msg.GetData<User>();
        }

        public async Task<RefreshTokenStatus> ValidateRefreshToken(Guid accountId, Guid refreshToken)
        {
            var msg = await _natsService.RequestAsync(new ValidateRefreshTokenRequest { AccountId = accountId, RefreshToken = refreshToken });
            return msg.GetData<RefreshTokenStatus>();
        }

        public async Task<User> UpdateRefreshToken(Guid userId)
        {
            var msg = await _natsService.RequestAsync(new UpdateRefreshTokenRequest { UserId = userId });
            return msg.GetData<User>();
        }

        public async Task UpdateUser(Guid platformUserId, MyIUser discordUser)
        {
            await _natsService.RequestAsync(new UpdateUserRequest { PlatformUserId = platformUserId, DiscordUser = discordUser });
        }
    }
}
