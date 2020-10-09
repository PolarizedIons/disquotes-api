using System;

namespace QuotesLib.Nats.Users
{
    public class ValidateRefreshTokenRequest : INatsRequest
    {
        public Guid AccountId { get; set; }
        public Guid RefreshToken { get; set; }
    }
}