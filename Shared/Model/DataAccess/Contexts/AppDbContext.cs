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
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ————————————————
        // Watcher ← MainCurator (1:N)
        modelBuilder.Entity<Watcher>()
            .HasOne(w => w.MainCurator)
            .WithMany(u => u.Watchers)
            .HasForeignKey(w => w.MainCuratorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Watcher ↔ Curators   (M:N)
        modelBuilder.Entity<Watcher>()
            .HasMany(w => w.Curators)
            .WithMany(u => u.CuratedWatchers)
            .UsingEntity(j => j.ToTable("WatcherCurators"));

        // Event  ← MainAdmin   (1:N)
        modelBuilder.Entity<Event>()
            .HasOne(e => e.MainAdmin)
            .WithMany(u => u.Events)
            .HasForeignKey(e => e.MainAdminId)
            .OnDelete(DeleteBehavior.Restrict);

        // Watcher ↔ Event      (M:N)
        modelBuilder.Entity<Watcher>()
            .HasMany(w => w.Participating)
            .WithMany(e => e.Participants)
            .UsingEntity(j => j.ToTable("EventParticipants"));

        // Record → Bird (N:1)
        modelBuilder.Entity<Record>()
            .HasOne(r => r.Bird)
            .WithMany()
            .HasForeignKey(r => r.BirdId);

        // Record → Watcher (N:1)
        modelBuilder.Entity<Record>()
            .HasOne<Watcher>(r => r.Watcher)
            .WithMany(w => w.Records)
            .HasForeignKey(r => r.WatcherId);

        var usr = new User() { Id = -1, IsAdmin = true, UserName = "string", PasswordHash = "string" };

        modelBuilder.Entity<User>().HasData(usr);

        base.OnModelCreating(modelBuilder);
    }
}

