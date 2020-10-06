using System;

namespace QuotesLib.Nats.Quotes
{
    public class DeleteQuoteRequest : INatsRequest
    {
        public Guid QuoteId { get; set; }
    }
}