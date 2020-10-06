namespace QuotesLib.Nats.Discord
{
    public class GetUserRequest : INatsRequest
    {
        public ulong DiscordId { get; set; }
    }
}
