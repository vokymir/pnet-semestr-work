namespace BirdWatching.Shared.Model;

public class Record
{
    public int Id { get; set; }
    public Bird Bird { get; set; } = new Bird();
    public DateTime DateSeen { get; set; } = DateTime.Now;
}
