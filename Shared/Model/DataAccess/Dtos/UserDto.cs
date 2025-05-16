namespace BirdWatching.Shared.Model;

public class UserDto : IAmDto<User>
{
    public int Id { get; set; }
    public string UserName { get; set; } = "username";
    public string PasswordHash { get; set; } = "password";
    public bool IsAdmin { get; set; } = false;

    public ICollection<WatcherDto> Watchers { get; set; } = new List<WatcherDto>();
    public ICollection<EventDto> Events { get; set; } = new List<EventDto>();

    public ICollection<WatcherDto> CuratedWatchers { get; set; } = new List<WatcherDto>();
    public ICollection<EventDto> AdministeredEvents { get; set; } = new List<EventDto>();
    public ICollection<AuthTokenDto> AuthTokens { get; set; } = new List<AuthTokenDto>();

    public User ToEntity()
    {
        var u = new User() {
            Id = Id,
            UserName = UserName,
            PasswordHash = PasswordHash,
            IsAdmin = IsAdmin
        };

        foreach (var w in Watchers)
            u.Watchers.Add(w.ToEntity());
        foreach (var e in Events)
            u.Events.Add(e.ToEntity());
        foreach (var w in CuratedWatchers)
            u.CuratedWatchers.Add(w.ToEntity());
        foreach (var e in AdministeredEvents)
            u.AdministeredEvents.Add(e.ToEntity());
        foreach (var a in AuthTokens)
            u.AuthTokens.Add(a.ToEntity());

        return u;
    }
}
