namespace QuotesLib.Nats.Users
{
    public class LoginDiscordUserRequest : INatsRequest
    {
        public IUser User { get; set; }
    }
}