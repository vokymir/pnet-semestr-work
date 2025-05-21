namespace BirdWatching.Shared.Model;

public class WatcherDto : IAmDto<Watcher>
{
    public int Id { get; set; }
    public string PublicIdentifier { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public int MainCuratorId { get; set; }
    public UserDto? MainCurator { get; set; }

    public ICollection<UserDto>? Curators { get; set; }
    public ICollection<EventDto>? Participating { get; set; }
    public ICollection<RecordDto>? Records { get; set; }

    public Watcher ToEntity()
    {
        var w = new Watcher() {
            Id = Id,
            FirstName = FirstName,
            LastName = LastName,
            PublicIdentifier = PublicIdentifier
        };

        return w;
    }
}
