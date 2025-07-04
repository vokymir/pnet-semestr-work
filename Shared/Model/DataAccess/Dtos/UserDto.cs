namespace BirdWatching.Shared.Model;

public class UserDto : IAmDto<User>
{
    public int Id { get; set; }
    public string Email { get; set; } = "email@email.com";
    public string UserName { get; set; } = "username";
    public string PasswordHash { get; set; } = "password";
    public bool IsAdmin { get; set; } = false;
    public int PreferenceLoginMinutes = -1;

    public string DisplayName { get; set; } = $"uzivatel-{DateTime.Now.ToString("dd.MM.yyyy HH:ss")}";

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
            IsAdmin = IsAdmin,
            Email = Email,
            DisplayName = DisplayName,
            PreferenceLoginMinutes = PreferenceLoginMinutes,
        };

        return u;
    }
}
