using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;
using QuotesApi.Database;
using QuotesApi.Extentions;
using QuotesApi.Models.Guilds;
using QuotesApi.Models.Users;

namespace QuotesApi.Services
{
    public class UserService : IScopedDiService
    {
        private DatabaseContext _db;
        private DiscordService _discord;

        public UserService(DatabaseContext databaseContext, DiscordService discordService)
        {
            _db = databaseContext;
            _discord = discordService;
        }

        public async Task<User> FindUser(Guid id, bool enrichWithGuilds = false)
        {
            var user = _db.Users.FirstOrDefault(x => x.Id == id && x.DeletedAt == null);

            if (user != null && enrichWithGuilds)
            {
                user.Guilds = (await _discord.GetGuildsFor(user.DiscordId)).Select(x => new Guild().MapProps(x));
            }

            return user;
        }

        public async Task<User> FindDiscordUser(ulong id, bool enrichWithGuilds = false)
        {
            var user = _db.Users.FirstOrDefault(x => x.DiscordId == id && x.DeletedAt == null);

            if (user != null && enrichWithGuilds)
            {
                user.Guilds = (await _discord.GetGuildsFor(user.DiscordId)).Select(x => new Guild().MapProps(x));
            }

            return user;
        }

        public async Task<User> CreateOrUpdateUser(RestUser discordUser)
        {
            var user = await FindDiscordUser(discordUser.Id);
            if (user != null)
            {
                user.Username = discordUser.Username;
                user.Discriminator = discordUser.DiscriminatorValue;
                user.ProfileUrl = discordUser.GetAvatarUrl();
            }
            else
            {
                await _db.Users.AddAsync(new User
                {
                    DiscordId = discordUser.Id,
                    Username = discordUser.Username,
                    Discriminator = discordUser.DiscriminatorValue,
                    ProfileUrl = discordUser.GetAvatarUrl(),
                });
            }

            await _db.SaveChangesAsync();
            return await FindDiscordUser(discordUser.Id);
        }
    }
}
