namespace BirdWatching;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.Data.Sqlite;

using BirdWatching.Shared;

public class App
{
    public ICollection<Bird> Birds { get; set; } = new List<Bird>();

    public static void Main(string[] args)
    {
        Console.WriteLine("Hello birdwatcher!");

        var connection = new SqliteConnectionStringBuilder() {
            DataSource = "db.db"
        };

        var appOptions = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection.ConnectionString).Options;

        var appContext = new AppDbContext(appOptions);

        appContext.Database.EnsureCreated();

        var birdRepo = new EFBirdRepository(appContext);
        var eventRepo = new EFEventRepository(appContext);
        var recordRepo = new EFRecordRepository(appContext);
        var userRepo = new EFUserRepository(appContext);
        var watcherRepo = new EFWatcherRepository(appContext);
    }
}

