namespace BirdWatching.Shared.Model;

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Bird> Birds { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Record> Records { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Watcher> Watchers { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
}
