namespace BirdWatching.Shared.Model;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = "Nepojmenovany event";
    public DateTime Start { get; set; } = DateTime.Now;
    public DateTime End { get; set; } = DateTime.MaxValue;
    public DateTime AddingDeadline { get; set; } = DateTime.MaxValue;
    public User MainAdmin { get; set; } = new User();

    public ICollection<Watcher> Participants { get; set; } = new List<Watcher>();
    public ICollection<User> Admins { get; set; } = new List<User>();
}
