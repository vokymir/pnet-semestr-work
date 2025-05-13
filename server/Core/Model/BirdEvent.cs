namespace BirdWatching;

public class BirdEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = "Nepojmenovany event";
    public DateTime Start { get; set; } = DateTime.Now;
    public DateTime End { get; set; } = DateTime.MaxValue;
    public DateTime AddingDeadline { get; set; } = DateTime.MaxValue;
    public BirdUser MainAdmin { get; set; } = new BirdUser();

    public ICollection<BirdWatcher> Participants { get; set; } = new List<BirdWatcher>();
    public ICollection<BirdUser> Admins { get; set; } = new List<BirdUser>();
}
