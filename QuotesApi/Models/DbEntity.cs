using System;

namespace QuotesApi.Models
{
    public class DbEntity
    {
        public Guid Id { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime LastModifiedAt { get; set; }
        
        public DateTime? DeletedAt { get; set; }

        protected DbEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = LastModifiedAt = DateTime.UtcNow;
        }
    }
}
