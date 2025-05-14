namespace BirdWatching.Shared.Model;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = "username";
    public string PasswordHash { get; set; } = "password";

    public ICollection<Watcher> Watchers { get; set; } = new List<Watcher>();
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
