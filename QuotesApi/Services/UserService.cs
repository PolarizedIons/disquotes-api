using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;
using QuotesApi.Database;
using QuotesApi.Exceptions;
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

            if (user == null)
            {
                throw new NotFoundException($"User with id '{id}' not found.");
            }

            if (enrichWithGuilds)
            {
                await SetGuids(user);
            }

            return user;
        }

        public async Task<User> FindDiscordUser(ulong id, bool enrichWithGuilds = false)
        {
            var user = _db.Users.FirstOrDefault(x => x.DiscordId == id && x.DeletedAt == null);

            if (user == null)
            {
                throw new NotFoundException($"User with discord id '{id}' not found.");
            }
            
            if (enrichWithGuilds)
            {
                await SetGuids(user);
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

        private async Task SetGuids(User user)
        {
            user.Guilds = (await _discord.GetGuildsFor(user.DiscordId)).Select(x => new Guild
            {
                Description = x.Description,
                Id = x.Id.ToString(),
                Name = x.Name,
                SystemChannelId = x.SystemChannelId.ToString(),
                IsOwner = x.OwnerId == user.DiscordId
            });
        }
    }
}
