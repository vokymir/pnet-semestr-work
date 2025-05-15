namespace BirdWatching.Shared.Model;

public class Record
{
    public int Id { get; set; }
    public DateTime DateSeen { get; set; }

    public int BirdId { get; set; }
    public Bird Bird { get; set; } = null!;

    public int WatcherId { get; set; }
}
