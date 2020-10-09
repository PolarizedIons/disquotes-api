using QuotesApi.Models.Users;
using QuotesLib.Models.Discord;

namespace QuotesLib.Nats.Users
{
    public class UpdateUserRequest : INatsRequest
    {
        public User PlatformUser { get; set; }
        public MyIUser DiscordUser { get; set; }
    }
}
