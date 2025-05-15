namespace BirdWatching.Shared.Model;

public class Watcher
{
    public int Id { get; set; }

    public int MainCuratorId { get; set; }
    public User MainCurator { get; set; } = null!;

    public ICollection<User> Curators { get; set; } = new List<User>();
    public ICollection<Event> Participating { get; set; } = new List<Event>();
    public ICollection<Record> Records { get; set; } = new List<Record>();
}
