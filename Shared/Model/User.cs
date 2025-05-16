namespace BirdWatching.Shared.Model;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = "username";
    public string PasswordHash { get; set; } = "password";
    public bool IsAdmin { get; set; } = false;

    // “Owns” these
    public ICollection<Watcher> Watchers { get; set; } = new List<Watcher>();
    public ICollection<Event> Events { get; set; } = new List<Event>();

    // M:N joins
    public ICollection<Watcher> CuratedWatchers { get; set; } = new List<Watcher>();
    public ICollection<Event> AdministeredEvents { get; set; } = new List<Event>();
    public ICollection<AuthToken> AuthTokens { get; set; } = new List<AuthToken>();
}
