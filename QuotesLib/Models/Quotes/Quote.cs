using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using QuotesApi.Models.Users;

namespace QuotesApi.Models.Quotes
{
    public class Quote : DbEntity, IQuote
    {
        [JsonIgnore]
        public Guid UserId { get; set; }
        [NotMapped]
        public UserDto User { get; set; }

        public string GuildId { get; set; }
        public bool Approved { get; set; }
        public int? QuoteNumber { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }

        public static Quote Merge(Quote quote, UserDto user)
        {
            quote.User = user;
            return quote;
        }
    }
}
