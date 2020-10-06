using System.Collections.Generic;
using System.Threading.Tasks;
using QuotesApi.Models.Quotes;

namespace QuotesLib.Services
{
    public interface IDiscordService
    {
        Task<bool> IsLoggedIn();
        Task<IEnumerable<IGuild>> GetMutualGuildsFor(ulong userId);
        Task<IGuild> GetGuild(ulong guildId);
        Task<IUser> GetUser(ulong userId);
        Task SendQuoteNotification(ulong guildId, Quote quote, ulong submitterId, ulong? approverId);
        Task<bool> IsModeratorInGuild(ulong userId, ulong guildId);
        Task<ISelfUser> GetUserFromAuthToken(string token);
    }
}
