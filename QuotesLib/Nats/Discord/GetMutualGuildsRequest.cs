namespace QuotesLib.Nats.Discord
{
    public class GetMutualGuildsRequest : INatsRequest
    {
        public ulong DiscordId { get; set; }
    }
}