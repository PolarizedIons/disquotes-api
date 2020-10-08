using System;

namespace QuotesApi.Models.Users
{
    public class UserDto : IUser
    {
        public Guid Id { get; set; }
        public string DiscordId { get; set; }
        public string Username { get; set; }
        public string Discriminator { get; set; }
        public string ProfileUrl { get; set; }
    }
}
