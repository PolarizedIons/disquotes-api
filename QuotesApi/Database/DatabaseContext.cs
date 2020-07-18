using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using QuotesApi.Models;
using QuotesApi.Models.Quotes;
using QuotesApi.Models.Users;

namespace QuotesApi.Database
{
    public sealed class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Quote> Quotes { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            ChangeTracker.StateChanged += OnEntityStateChanged;
        }

        private void OnEntityStateChanged(object sender, EntityStateChangedEventArgs e)
        {
            if (e.NewState == EntityState.Modified && e.Entry.Entity is DbEntity entity)
            {
                entity.LastModifiedAt = DateTime.Now;   
            }
        }
    }
}
