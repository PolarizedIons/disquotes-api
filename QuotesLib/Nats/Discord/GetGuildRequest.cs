namespace QuotesLib.Nats.Discord
{
    public class GetGuildRequest : INatsRequest
    {
        public ulong DiscordId { get; set; }
    }
}