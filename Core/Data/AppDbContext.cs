using Core.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data
{
    /// <summary>
    /// Database context for the application.
    /// Must have, for Entity Framework to work.
    /// Setup the tables of database kinda.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ENTITIES
        public DbSet<User> Users { get; set; }
        public DbSet<Watcher> Watchers { get; set; }
        public DbSet<Contest> Contests { get; set; }
        public DbSet<Entry> Entries { get; set; }

        // JOIN TABLES
        public DbSet<UserWatcher> UserWatchers { get; set; }
        public DbSet<UserContest> UserContests { get; set; }
        public DbSet<WatcherContest> WatcherContests { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("TestDb");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Entry>().HasKey(["ContestId", "WatcherId", "Timestamp"]);

        }
    }
}
