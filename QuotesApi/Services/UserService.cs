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
        private static readonly TimeSpan RefreshTokenValidFor = TimeSpan.FromDays(14);
        
        private DatabaseContext _db;
        private DiscordService _discord;

        public UserService(DatabaseContext databaseContext, DiscordService discordService)
        {
            _db = databaseContext;
            _discord = discordService;
        }

        public async Task<User> FindUser(Guid id, bool enrichWithGuilds = false, bool throwNotfound = true)
        {
            var user = _db.Users.FirstOrDefault(x => x.Id == id && x.DeletedAt == null);

            if (user == null && throwNotfound)
            {
                throw new NotFoundException($"User with id '{id}' not found.");
            }

            if (enrichWithGuilds)
            {
                await SetGuildsOn(user);
            }

            return user;
        }

        public async Task<User> FindDiscordUser(ulong id, bool enrichWithGuilds = false, bool throwNotfound = true)
        {
            var user = _db.Users.FirstOrDefault(x => x.DiscordId == id && x.DeletedAt == null);

            if (user == null && throwNotfound)
            {
                throw new NotFoundException($"User with discord id '{id}' not found.");
            }
            
            if (enrichWithGuilds)
            {
                await SetGuildsOn(user);
            }

            return user;
        }

        public async Task<User> LoginDiscordUser(RestUser discordUser)
        {
            var user = await FindDiscordUser(discordUser.Id, throwNotfound: false);
            if (user != null)
            {
                user.Username = discordUser.Username;
                user.Discriminator = discordUser.DiscriminatorValue;
                user.ProfileUrl = discordUser.GetAvatarUrl();
                user.RefreshToken = Guid.NewGuid();
                user.RefreshTokenExpires = DateTime.UtcNow.Add(RefreshTokenValidFor);
            }
            else
            {
                await _db.Users.AddAsync(new User
                {
                    DiscordId = discordUser.Id,
                    Username = discordUser.Username,
                    Discriminator = discordUser.DiscriminatorValue,
                    ProfileUrl = discordUser.GetAvatarUrl(),
                    RefreshToken = Guid.NewGuid(),
                    RefreshTokenExpires = DateTime.UtcNow.Add(RefreshTokenValidFor),
                });
            }

            await _db.SaveChangesAsync();
            return await FindDiscordUser(discordUser.Id);
        }

        private async Task SetGuildsOn(User user)
        {
            user.Guilds = (await _discord.GetGuildsFor(user.DiscordId))
                .Select(x => new Guild
                    {
                        Description = x.Description,
                        Id = x.Id.ToString(),
                        Name = x.Name,
                        SystemChannelId = x.SystemChannelId.ToString(),
                        IsOwner = x.OwnerId == user.DiscordId,
                        IconUrl = x.IconUrl,
                    }
                );
        }

        public void ValidateRefreshToken(Guid accountId, Guid refreshToken)
        {
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
            var user = await FindUser(accountId);
            user.RefreshToken = Guid.NewGuid();
            user.RefreshTokenExpires = DateTime.UtcNow.Add(RefreshTokenValidFor);
            await _db.SaveChangesAsync();
            return user;
        }
    }
}
