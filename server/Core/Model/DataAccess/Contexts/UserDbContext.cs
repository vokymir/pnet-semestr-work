namespace BirdWatching;
using Microsoft.EntityFrameworkCore;

public class UserDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public UserDbContext(DbContextOptions<BirdDbContext> options) : base(options)
    {

    }
}
