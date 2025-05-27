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

        if (u.Watchers is null) u.Watchers = new List<WatcherDto>();
        foreach (var w in Watchers)
            u.Watchers.Add(w.ToDto());

        if (u.Events is null) u.Events = new List<EventDto>();
        foreach (var e in Events)
            u.Events.Add(e.ToDto());

        if (u.CuratedWatchers is null) u.CuratedWatchers = new List<WatcherDto>();
        foreach (var w in CuratedWatchers)
            u.CuratedWatchers.Add(w.ToDto());

        if (u.AdministeredEvents is null) u.AdministeredEvents = new List<EventDto>();
        foreach (var e in AdministeredEvents)
            u.AdministeredEvents.Add(e.ToDto());

        return u;
    }
}
