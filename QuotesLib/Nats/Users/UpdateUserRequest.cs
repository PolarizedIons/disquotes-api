using QuotesApi.Models.Users;
using IUser = Discord.IUser;

namespace QuotesLib.Nats.Users
{
    public class UpdateUserRequest : INatsRequest
    {
        public User PlatformUser { get; set; }
        public IUser DiscordUser { get; set; }
    }
}