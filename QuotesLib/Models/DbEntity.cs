using System;
using System.Text.Json.Serialization;

namespace QuotesApi.Models
{
    public class DbEntity
    {
        public Guid Id { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        [JsonIgnore]
        public DateTime LastModifiedAt { get; set; }
        
        [JsonIgnore]
        public DateTime? DeletedAt { get; set; }
    }
}
