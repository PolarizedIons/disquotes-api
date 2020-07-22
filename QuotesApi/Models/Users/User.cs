using System;
using System.Text.Json.Serialization;

namespace QuotesApi.Models.Users
{
    public class User : DbEntity, IUser
    {
        public string DiscordId { get; set; }
        public string Username { get; set; }
        public string Discriminator { get; set; }
        public string ProfileUrl { get; set; }
        
        [JsonIgnore]
        public Guid? RefreshToken { get; set; }

        [JsonIgnore]
        public DateTime? RefreshTokenExpires { get; set; }
    }
}
