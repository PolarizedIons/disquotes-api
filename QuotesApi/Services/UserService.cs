using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;
using QuotesApi.Database;
using QuotesApi.Exceptions;
using QuotesApi.Models.Users;

namespace QuotesApi.Services
{
    public class UserService : IScopedDiService
    {
        private static readonly TimeSpan RefreshTokenValidFor = TimeSpan.FromDays(14);
        
        private DatabaseContext _db;

        public UserService(DatabaseContext databaseContext)
        {
            _db = databaseContext;
        }

        public IEnumerable<User> FindAllUsers()
        {
            return _db.Users.AsQueryable().Where(x => x.DeletedAt == null);
        }

        public User FindUser(Guid id, bool throwNotfound = true)
        {
            var user = _db.Users.FirstOrDefault(x => x.Id == id && x.DeletedAt == null);

            if (user == null && throwNotfound)
            {
                throw new NotFoundException($"User with id '{id}' not found.");
            }

            return user;
        }

        public async Task<User> FindDiscordUser(string id, bool throwNotfound = true)
        {
            var user = _db.Users.FirstOrDefault(x => x.DiscordId == id && x.DeletedAt == null);

            if (user == null && throwNotfound)
            {
                throw new NotFoundException($"User with discord id '{id}' not found.");
            }

            return user;
        }

        public async Task<User> LoginDiscordUser(RestUser discordUser)
        {
            var user = await FindDiscordUser(discordUser.Id.ToString(), false);
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
            return await FindDiscordUser(discordUser.Id.ToString());
        }

        public void ValidateRefreshToken(Guid accountId, Guid refreshToken)
        {
            if (accountId == Guid.Empty || refreshToken == Guid.Empty)
            {
                throw new UnauthorizedException("You must provide both a accountId and refreshToken.");
            }

            var user = _db.Users
                .AsQueryable()
                .FirstOrDefault(x => x.Id == accountId && x.DeletedAt == null && x.RefreshToken == refreshToken);
            if (user == null)
            {
                throw new UnauthorizedException("AccountID and Refresh Token combination is invalid.");
            }

            if (user.RefreshTokenExpires < DateTime.UtcNow)
            {
                throw new UnauthorizedException("Refresh Token has expired");
            }
        }

        public async Task<User> UpdateRefreshToken(Guid accountId)
        {
            var user = FindUser(accountId);
            user.RefreshToken = Guid.NewGuid();
            user.RefreshTokenExpires = DateTime.UtcNow.Add(RefreshTokenValidFor);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUser(User user, RestUser discordUser)
        {
            user.Username = discordUser.Username;
            user.Discriminator = discordUser.Discriminator;
            user.ProfileUrl = discordUser.GetAvatarUrl();
            await _db.SaveChangesAsync();
        }
    }
}
