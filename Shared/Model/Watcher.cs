namespace BirdWatching.Shared.Model;

public class Watcher
{
    public int Id { get; set; }

    public ICollection<Record> Records = new List<Record>();

    public User MainCurator { get; set; } = new User();
    public ICollection<User> Curators = new List<User>();

    public ICollection<Event> Participating = new List<Event>();
}
