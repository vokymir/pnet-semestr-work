namespace BirdWatching.Shared.Model;

public class WatcherDto : IAmDto<Watcher>
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public int MainCuratorId { get; set; }

    public ICollection<EventDto> Participating { get; set; } = new List<EventDto>();
    public ICollection<RecordDto> Records { get; set; } = new List<RecordDto>();

    public Watcher ToEntity()
    {
        var w = new Watcher() {
            Id = Id,
            FirstName = FirstName,
            LastName = LastName,
            MainCuratorId = MainCuratorId,
        };

        foreach (var p in Participating)
            w.Participating.Add(p.ToEntity());
        foreach (var r in Records)
            w.Records.Add(r.ToEntity());

        return w;
    }
}
