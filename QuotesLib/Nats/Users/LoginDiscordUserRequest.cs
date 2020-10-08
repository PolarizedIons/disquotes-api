using QuotesLib.Models.Discord;

namespace QuotesLib.Nats.Users
{
    public class LoginDiscordUserRequest : INatsRequest
    {
        public MyIUser User { get; set; }
    }
}
