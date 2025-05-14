namespace BirdWatching;
using Microsoft.EntityFrameworkCore;

public class WatcherDbContext : DbContext
{
    public DbSet<Watcher> Watchers { get; set; }

    public WatcherDbContext(DbContextOptions<WatcherDbContext> options) : base(options)
    {

    }
}
