namespace QuotesLib.Nats.Discord
{
    public class IsModeratorInGuildRequest : INatsRequest
    {
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
    }
}
