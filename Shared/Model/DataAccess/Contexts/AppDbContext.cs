namespace BirdWatching.Shared.Model;

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Bird> Birds { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Record> Records { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Watcher> Watchers { get; set; }
    public DbSet<AuthToken> AuthTokens { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { Database.EnsureCreated(); }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ————————————————
        // AuthToken - User (many-to-one)
        modelBuilder.Entity<AuthToken>()
            .HasOne(t => t.User)
            .WithMany(u => u.AuthTokens)
            .IsRequired();

        // Watcher ← MainCurator (1:N)
        modelBuilder.Entity<Watcher>()
            .HasOne<User>() // No navigation property on Watcher
            .WithMany(u => u.Watchers) // Navigation property on User
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

        // Event  ↔ Admins      (M:N)
        modelBuilder.Entity<Event>()
            .HasMany(e => e.Admins)
            .WithMany(u => u.AdministeredEvents)
            .UsingEntity(j => j.ToTable("EventAdmins"));

        // Watcher ↔ Event      (M:N)
        modelBuilder.Entity<Watcher>()
            .HasMany(w => w.Participating)
            .WithMany(e => e.Participants)
            .UsingEntity(j => j.ToTable("EventParticipants"));

        // Record → Bird (N:1)
        modelBuilder.Entity<Record>()
            .HasOne(r => r.Bird)
            .WithMany()             // if you don’t track Bird→Records, leave it empty
            .HasForeignKey("BirdId");

        // Record → Watcher (N:1)
        modelBuilder.Entity<Record>()
            .HasOne<Watcher>()
            .WithMany(w => w.Records)
            .HasForeignKey("WatcherId");

        var usr = new User() { Id = -1, IsAdmin = true, UserName = "string", PasswordHash = "string" };

        modelBuilder.Entity<User>().HasData(usr);

        base.OnModelCreating(modelBuilder);
    }
}

