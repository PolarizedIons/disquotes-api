using System;

namespace QuotesApi.Models.Users
{
    public class User : DbEntity, IUser
    {
        public string DiscordId { get; set; }
        public string Username { get; set; }
        public string Discriminator { get; set; }
        public string ProfileUrl { get; set; }
        public Guid? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpires { get; set; }
    }
}
