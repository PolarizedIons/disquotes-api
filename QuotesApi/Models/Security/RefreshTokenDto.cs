using System;

namespace QuotesApi.Models.Security
{
    public class RefreshTokenDto
    {
        public Guid AccountId { get; set; }
        public Guid RefreshToken { get; set; }
    }
}
