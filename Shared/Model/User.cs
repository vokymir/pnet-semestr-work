namespace BirdWatching.Shared.Model;

public class User : IHaveDto<UserDto>
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

    public UserDto ToDto()
    {
        var u = new UserDto() {
            Id = Id,
            UserName = UserName,
            PasswordHash = PasswordHash,
            IsAdmin = IsAdmin
        };

        return u;
    }
    public UserDto ToFullDto()
    {
        var u = new UserDto() {
            Id = Id,
            UserName = UserName,
            PasswordHash = PasswordHash,
            IsAdmin = IsAdmin
        };

        foreach (var w in Watchers)
            u.Watchers.Add(w.ToDto());
        foreach (var e in Events)
            u.Events.Add(e.ToDto());
        foreach (var w in CuratedWatchers)
            u.CuratedWatchers.Add(w.ToDto());
        foreach (var e in AdministeredEvents)
            u.AdministeredEvents.Add(e.ToDto());
        foreach (var a in AuthTokens)
            u.AuthTokens.Add(a.ToDto());

        return u;
    }
}
