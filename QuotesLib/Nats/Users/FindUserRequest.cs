using System;

namespace QuotesLib.Nats.Users
{
    public class FindUserRequest : INatsRequest
    {
        public Guid UserId { get; set; }
    }
}