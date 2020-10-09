using System;
using System.Threading.Tasks;
using QuotesApi.Models.Users;
using QuotesLib.Models.Discord;
using QuotesLib.Models.Security;

namespace QuotesLib.Services
{
    public interface IUserService
    {
        Task<User> FindUser(Guid userId);
        Task<User> FindDiscordUser(ulong discordId);
        Task<User> LoginDiscordUser(MyIUser discordUser);
        Task<RefreshTokenStatus> ValidateRefreshToken(Guid accountId, Guid refreshToken);
        Task<User> UpdateRefreshToken(Guid userId);
        Task UpdateUser(Guid userId, MyIUser discordUser);
    }
}
