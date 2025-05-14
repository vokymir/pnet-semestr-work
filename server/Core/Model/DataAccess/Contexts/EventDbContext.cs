namespace BirdWatching;
using Microsoft.EntityFrameworkCore;

public class EventDbContext : DbContext
{
    public DbSet<Event> Events { get; set; }

    public EventDbContext(DbContextOptions<EventDbContext> options) : base(options)
    {

    }
}
