using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuotesApi.Models.Users;
using QuotesCore.Database;
using QuotesLib.Models;
using QuotesLib.Models.Discord;
using QuotesLib.Models.Security;
using QuotesLib.Services;

namespace QuotesCore.Services
{
    public class UserService : IUserService, IScopedDiService
    {
        private static readonly TimeSpan RefreshTokenValidFor = TimeSpan.FromDays(14);
        
        private DatabaseContext _db;

        public UserService(DatabaseContext databaseContext)
        {
            _db = databaseContext;
        }

        public Task<IEnumerable<User>> FindAllUsers()
        {
            var result = _db.Users.AsQueryable()
                .Where(x => x.DeletedAt == null)
                .ToList();
            return Task.FromResult<IEnumerable<User>>(result);
        }

        public Task<User> FindUser(Guid userId)
        {
            return Task.FromResult(_db.Users.FirstOrDefault(x => x.Id == userId && x.DeletedAt == null));
        }

        public Task<User> FindDiscordUser(ulong discordId)
        {
            return Task.FromResult(_db.Users.FirstOrDefault(x => x.DiscordId == discordId.ToString() && x.DeletedAt == null));
        }

        public async Task<User> LoginDiscordUser(MyIUser discordUser)
        {
            var user = await FindDiscordUser(discordUser.Id);
            if (user != null)
            {
                user.Username = discordUser.Username;
                user.Discriminator = discordUser.Discriminator;
                user.ProfileUrl = discordUser.GetAvatarUrl();
                user.RefreshToken = Guid.NewGuid();
                user.RefreshTokenExpires = DateTime.UtcNow.Add(RefreshTokenValidFor);
            }
            else
            {
                await _db.Users.AddAsync(new User
                {
                    DiscordId = discordUser.Id.ToString(),
                    Username = discordUser.Username,
                    Discriminator = discordUser.Discriminator,
                    ProfileUrl = discordUser.GetAvatarUrl(),
                    RefreshToken = Guid.NewGuid(),
                    RefreshTokenExpires = DateTime.UtcNow.Add(RefreshTokenValidFor),
                });
            }

            await _db.SaveChangesAsync();
            return await FindDiscordUser(discordUser.Id);
        }

        public Task<RefreshTokenStatus> ValidateRefreshToken(Guid accountId, Guid refreshToken)
        {
            if (accountId == Guid.Empty || refreshToken == Guid.Empty)
            {
                return Task.FromResult(RefreshTokenStatus.NOT_PROVIDED);
            }

            var user = _db.Users
                .AsQueryable()
                .FirstOrDefault(x => x.Id == accountId && x.DeletedAt == null && x.RefreshToken == refreshToken);
            if (user == null)
            {
                return Task.FromResult(RefreshTokenStatus.INVALID_COMBINATION);
            }

            if (user.RefreshTokenExpires < DateTime.UtcNow)
            {
                return Task.FromResult(RefreshTokenStatus.EXPIRED);
            }

            return Task.FromResult(RefreshTokenStatus.VALID);
        }

        public async Task<User> UpdateRefreshToken(Guid userId)
        {
            var user = await FindUser(userId);
            user.RefreshToken = Guid.NewGuid();
            user.RefreshTokenExpires = DateTime.UtcNow.Add(RefreshTokenValidFor);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUser(User user, MyIUser discordUser)
        {
            user.Username = discordUser.Username;
            user.Discriminator = discordUser.Discriminator;
            user.ProfileUrl = discordUser.GetAvatarUrl();
            await _db.SaveChangesAsync();
        }
    }
}
