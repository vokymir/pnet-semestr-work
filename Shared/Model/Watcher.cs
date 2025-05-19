namespace BirdWatching.Shared.Model;

public class Watcher : IHaveDto<WatcherDto>
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public int MainCuratorId { get; set; }
    public User MainCurator { get; set; } = null!;

    public ICollection<User> Curators { get; set; } = new List<User>();
    public ICollection<Event> Participating { get; set; } = new List<Event>();
    public ICollection<Record> Records { get; set; } = new List<Record>();

    public WatcherDto ToDto()
    {
        var w = new WatcherDto() {
            Id = Id,
            FirstName = FirstName,
            LastName = LastName,
            MainCuratorId = MainCuratorId,
        };

        return w;
    }
    public WatcherDto ToFullDto()
    {
        var w = new WatcherDto() {
            Id = Id,
            FirstName = FirstName,
            LastName = LastName,
            MainCuratorId = MainCuratorId,
        };

        if (MainCurator is not null)
            w.MainCurator = MainCurator.ToDto();

        if (w.Curators is null) w.Curators = new List<UserDto>();
        foreach (var c in Curators)
            w.Curators.Add(c.ToDto());

        if (w.Participating is null) w.Participating = new List<EventDto>();
        foreach (var p in Participating)
            w.Participating.Add(p.ToDto());

        if (w.Records is null) w.Records = new List<RecordDto>();
        foreach (var r in Records)
            w.Records.Add(r.ToDto());

        return w;
    }
}
