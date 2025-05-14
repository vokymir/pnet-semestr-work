namespace BirdWatching;
using Microsoft.EntityFrameworkCore;

public class RecordDbContext : DbContext
{
    public DbSet<Record> Records { get; set; }

    public RecordDbContext(DbContextOptions<BirdDbContext> options) : base(options)
    {

    }
}
