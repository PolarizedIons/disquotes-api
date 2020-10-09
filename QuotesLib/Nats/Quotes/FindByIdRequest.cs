using System;

namespace QuotesLib.Nats.Quotes
{
    public class FindByIdRequest : INatsRequest
    {
        public Guid QuoteId { get; set; }
        public bool OnlyApproved { get; set; }
        public bool EnrichWithUser { get; set; } 
    }
}