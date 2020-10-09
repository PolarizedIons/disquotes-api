using System;
using QuotesLib.Models.Discord;

namespace QuotesLib.Nats.Users
{
    public class UpdateUserRequest : INatsRequest
    {
        public Guid PlatformUserId { get; set; }
        public MyIUser DiscordUser { get; set; }
    }
}
