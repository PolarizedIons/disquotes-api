using System;

namespace QuotesLib.Nats.Users
{
    public class UpdateRefreshTokenRequest : INatsRequest
    {
        public Guid UserId { get; set; }
    }
}