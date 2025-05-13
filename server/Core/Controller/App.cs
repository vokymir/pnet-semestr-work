namespace BirdWatching;

public class App
{
    public ICollection<Bird> Birds { get; set; } = new List<Bird>();
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello birdwatcher!");
    }
}

