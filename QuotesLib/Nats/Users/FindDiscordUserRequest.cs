namespace QuotesLib.Nats.Users
{
    public class FindDiscordUserRequest : INatsRequest
    {
        public ulong UserId { get; set; }
    }
}