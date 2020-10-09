using System;
using QuotesApi.Models.Quotes;

namespace QuotesLib.Nats.Quotes
{
    public class AddQuoteRequest : INatsRequest
    {
        public QuoteDto Quote { get; set; }
        public Guid UserId { get; set; }
    }
}
