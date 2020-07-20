using System;

namespace QuotesApi.Models.Quotes
{
    public class Quote : DbEntity, IQuote
    {
        public Guid UserId { get; set; }
        public string GuildId { get; set; }
        public bool Approved { get; set; }
        public int? QuoteNumber { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
    }
}
