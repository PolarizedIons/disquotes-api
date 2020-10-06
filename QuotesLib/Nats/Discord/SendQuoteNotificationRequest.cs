using QuotesApi.Models.Quotes;

namespace QuotesLib.Nats.Discord
{
    public class SendQuoteNotificationRequest : INatsRequest
    {
        public ulong GuildId { get; set; }
        public Quote Quote { get; set; }
        public ulong SubmitterId { get; set; }
        public ulong? ApproverId { get; set; }
    }
}