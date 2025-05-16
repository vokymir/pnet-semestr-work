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
            MainCurator = MainCurator.ToDto()
        };

        foreach (var c in Curators)
            w.Curators.Add(c.ToDto());
        foreach (var p in Participating)
            w.Participating.Add(p.ToDto());
        foreach (var r in Records)
            w.Records.Add(r.ToDto());

        return w;
    }
}
