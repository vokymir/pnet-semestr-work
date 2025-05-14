namespace BirdWatching;
using Microsoft.EntityFrameworkCore;

public class BirdDbContext : DbContext
{
    public DbSet<Bird> Birds { get; set; }

    public BirdDbContext(DbContextOptions<BirdDbContext> options) : base(options)
    {

    }
}
