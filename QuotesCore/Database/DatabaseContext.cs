using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using QuotesApi.Models;
using QuotesApi.Models.Quotes;
using QuotesApi.Models.Users;

namespace QuotesCore.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Quote> Quotes { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            ChangeTracker.StateChanged += OnEntityStateChanged;
            ChangeTracker.Tracked += OnEntityTracked;
        }

        private void OnEntityTracked(object sender, EntityTrackedEventArgs e)
        {
            if (e.Entry.State == EntityState.Added && e.Entry.Entity is DbEntity entity)
            {
                entity.Id = Guid.NewGuid();
                entity.CreatedAt = DateTime.UtcNow;
                entity.LastModifiedAt = DateTime.UtcNow;
            }
        }

        private static void OnEntityStateChanged(object sender, EntityStateChangedEventArgs e)
        {
            if (e.NewState == EntityState.Modified && e.Entry.Entity is DbEntity entity)
            {
                entity.LastModifiedAt = DateTime.UtcNow;
            }
        }
    }
}
