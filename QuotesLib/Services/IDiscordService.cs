using System.Collections.Generic;
using System.Threading.Tasks;
using QuotesApi.Models.Quotes;
using QuotesLib.Models.Discord;

namespace QuotesLib.Services
{
    public interface IDiscordService
    {
        Task<bool> IsLoggedIn();
        Task<IEnumerable<MyIGuild>> GetMutualGuildsFor(ulong userId);
        Task<MyIGuild> GetGuild(ulong guildId);
        Task<MyIUser> GetUser(ulong userId);
        Task SendQuoteNotification(ulong guildId, Quote quote, ulong submitterId, ulong? approverId);
        Task<bool> IsModeratorInGuild(ulong userId, ulong guildId);
        Task<MyISelfUser> GetUserFromAuthToken(string token);
    }
}
