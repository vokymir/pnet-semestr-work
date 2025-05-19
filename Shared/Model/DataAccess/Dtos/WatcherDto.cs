namespace BirdWatching.Shared.Model;

public class WatcherDto : IAmDto<Watcher>
{
    public int Id { get; set; }
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
            MainCuratorId = MainCuratorId,
        };
        if (MainCurator is not null)
            w.MainCurator = MainCurator.ToEntity();

        if (Curators is not null)
            foreach (var c in Curators)
                w.Curators.Add(c.ToEntity());

        if (Participating is not null)
            foreach (var p in Participating)
                w.Participating.Add(p.ToEntity());

        if (Records is not null)
            foreach (var r in Records)
                w.Records.Add(r.ToEntity());

        return w;
    }
}
