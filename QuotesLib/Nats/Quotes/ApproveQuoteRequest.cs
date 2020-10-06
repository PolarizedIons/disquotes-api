using System;

namespace QuotesLib.Nats.Quotes
{
    public class ApproveQuoteRequest : INatsRequest
    {
        public Guid QuoteId { get; set; }
        public Guid ApproverId { get; set; }
    }
}
