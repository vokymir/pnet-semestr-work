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

    public User ToEntity()
    {
        var u = new User() {
            Id = Id,
            UserName = UserName,
            PasswordHash = PasswordHash,
            IsAdmin = IsAdmin
        };

        return u;
    }
}
