namespace BirdWatching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.Data.Sqlite;

public class App
{
    public ICollection<Bird> Birds { get; set; } = new List<Bird>();

    public static void Main(string[] args)
    {
        Console.WriteLine("Hello birdwatcher!");

        var connection = new SqliteConnectionStringBuilder()
        {
            DataSource = "db.db"
        };

        var appOptions = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection.ConnectionString).Options;
        // var birdOptions = new DbContextOptionsBuilder<BirdDbContext>().UseSqlite(connection.ConnectionString).Options;
        // var eventOptions = new DbContextOptionsBuilder<EventDbContext>().UseSqlite(connection.ConnectionString).Options;
        // var recordOptions = new DbContextOptionsBuilder<RecordDbContext>().UseSqlite(connection.ConnectionString).Options;
        // var userOptions = new DbContextOptionsBuilder<UserDbContext>().UseSqlite(connection.ConnectionString).Options;
        // var watcherOptions = new DbContextOptionsBuilder<WatcherDbContext>().UseSqlite(connection.ConnectionString).Options;

        var appContext = new AppDbContext(appOptions);
        // var birdContext = new BirdDbContext(birdOptions);
        // var eventContext = new EventDbContext(eventOptions);
        // var recordContext = new RecordDbContext(recordOptions);
        // var userContext = new UserDbContext(userOptions);
        // var watcherContext = new WatcherDbContext(watcherOptions);

        appContext.Database.EnsureCreated();
        // birdContext.Database.EnsureCreated();
        // eventContext.Database.EnsureCreated();
        // recordContext.Database.EnsureCreated();
        // userContext.Database.EnsureCreated();
        // watcherContext.Database.EnsureCreated();

        var birdRepo = new EFBirdRepository(appContext);
        var eventRepo = new EFEventRepository(appContext);
        var recordRepo = new EFRecordRepository(appContext);
        var userRepo = new EFUserRepository(appContext);
        var watcherRepo = new EFWatcherRepository(appContext);
        // var birdRepo = new EFBirdRepository(birdContext);
        // var eventRepo = new EFEventRepository(eventContext);
        // var recordRepo = new EFRecordRepository(recordContext);
        // var userRepo = new EFUserRepository(userContext);
        // var watcherRepo = new EFWatcherRepository(watcherContext);
    }
}

