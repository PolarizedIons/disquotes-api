namespace QuotesApi.Models.Quotes
{
    public class QuoteDto : IQuote
    {
        public string GuildId { get; set; }
        public string Text { get; set; }
    }
}
