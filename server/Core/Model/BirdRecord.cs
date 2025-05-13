namespace BirdWatching;

public class BirdRecord
{
    public int Id { get; set; }
    public Bird Bird { get; set; } = new Bird();
    public DateTime DateSeen { get; set; } = DateTime.Now;
}
