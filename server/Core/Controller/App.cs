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

        var options = new DbContextOptionsBuilder<StagDbContext>().UseSqlite(connection.ConnectionString).Options;

        var context = new StagDbContext(options);
        context.Database.EnsureCreated();

        var repo = new EFStudentRepository(context);
        var repo = new EFStudentRepository(context);
    }
}

