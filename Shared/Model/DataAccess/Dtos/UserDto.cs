namespace BirdWatching.Shared.Model;

public class UserDto : IAmDto<User>
{
    public int Id { get; set; }
    public string UserName { get; set; } = "username";
    public string PasswordHash { get; set; } = "password";
    public bool IsAdmin { get; set; } = false;

    public ICollection<WatcherDto>? Watchers { get; set; }
    public ICollection<EventDto>? Events { get; set; }

    public ICollection<WatcherDto>? CuratedWatchers { get; set; }
    public ICollection<EventDto>? AdministeredEvents { get; set; }
    public ICollection<AuthTokenDto>? AuthTokens { get; set; }

    public User ToEntity()
    {
        var u = new User() {
            Id = Id,
            UserName = UserName,
            PasswordHash = PasswordHash,
            IsAdmin = IsAdmin
        };

        if (Watchers is not null)
            foreach (var w in Watchers)
                u.Watchers.Add(w.ToEntity());

        if (Events is not null)
            foreach (var e in Events)
                u.Events.Add(e.ToEntity());

        if (CuratedWatchers is not null)
            foreach (var w in CuratedWatchers)
                u.CuratedWatchers.Add(w.ToEntity());

        if (AdministeredEvents is not null)
            foreach (var e in AdministeredEvents)
                u.AdministeredEvents.Add(e.ToEntity());

        if (AuthTokens is not null)
            foreach (var a in AuthTokens)
                u.AuthTokens.Add(a.ToEntity());

        return u;
    }
}
