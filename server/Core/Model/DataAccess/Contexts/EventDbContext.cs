namespace BirdWatching;
using Microsoft.EntityFrameworkCore;

public class EventDbContext : DbContext
{
    public DbSet<Event> Events { get; set; }

    public EventDbContext(DbContextOptions<BirdDbContext> options) : base(options)
    {

    }
}
