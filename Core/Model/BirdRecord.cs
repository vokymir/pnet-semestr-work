namespace BirdWatching;

public class BirdRecord
{
    public Bird Bird { get; set; } = new Bird("Neurceny", "Ptacek");
    public DateTime DateSeen { get; set; } = DateTime.Now;

    public BirdRecord() { }
}
