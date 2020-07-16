using System;
using System.Linq;
using System.Threading.Tasks;
using QuotesApi.Database;
using QuotesApi.Models.Users;
using IUser = Discord.IUser;

namespace QuotesApi.Services
{
    public class UserService : IDIService
    {
        private DatabaseContext _db;

        public UserService(DatabaseContext databaseContext)
        {
            _db = databaseContext;
        }

        public async Task<User> FindUser(Guid id)
        {
            return _db.Users.First(x => x.Id == id && x.DeletedAt == null);
        }

        public async Task<User> FindDiscordUser(ulong id)
        {
            return _db.Users.FirstOrDefault(x => x.DiscordId == id && x.DeletedAt == null);
        }

        public async Task<User> CreateOrUpdateUser(IUser discordUser)
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
                    ProfileUrl = discordUser.GetAvatarUrl()
                });
            }

            await _db.SaveChangesAsync();
            return await FindDiscordUser(discordUser.Id);
        }
    }
}
