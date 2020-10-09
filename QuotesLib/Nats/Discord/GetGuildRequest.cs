namespace QuotesLib.Nats.Discord
{
    public class GetGuildRequest : INatsRequest
    {
        public ulong GuildId { get; set; }
    }
}