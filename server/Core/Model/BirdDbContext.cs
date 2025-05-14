namespace BirdWatching;
using Microsoft.EntityFrameworkCore;

public class BirdDbContext : DbContext
{
    public DbSet<Bird> Birds { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Record> Records { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Watcher> Watchers { get; set; }

    public BirdDbContext(DbContextOptions<BirdDbContext> options) : base(options)
    {

    }

}
