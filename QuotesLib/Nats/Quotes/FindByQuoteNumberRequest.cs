namespace QuotesLib.Nats.Quotes
{
    public class FindByQuoteNumberRequest : INatsRequest
    {
        public ulong GuildId { get; set; }
        public int QuoteNumber { get; set; }
        public bool OnlyApproved { get; set; }
        public bool EnrichWithUser { get; set; }
    }
}