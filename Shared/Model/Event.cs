namespace BirdWatching.Shared.Model;

public class Event : IHaveDto<EventDto>
{
    public int Id { get; set; }
    public string PublicIdentifier { get; set; } = string.Empty;

    public string Name { get; set; } = "Nepojmenovany event";
    // event parameters
    public DateTime Start { get; set; } = DateTime.Now;
    public DateTime End { get; set; } = DateTime.MaxValue;
    public bool AllowDuplicates { get; set; } = false; // wheter to allow having the same bird twice in an event
    public string OrdoRegex { get; set; } = "*";
    public string FamiliaRegex { get; set; } = "*";
    public string GenusRegex { get; set; } = "*";
    public string SpeciesRegex { get; set; } = "*";

    public int MainAdminId { get; set; }
    public User MainAdmin { get; set; } = null!;

    public ICollection<Watcher> Participants { get; set; } = new List<Watcher>();

    public EventDto ToDto()
    {
        var e = new EventDto() {
            Id = Id,
            Name = Name,
            Start = Start,
            End = End,
            MainAdminId = MainAdminId,
            PublicIdentifier = PublicIdentifier,
            AllowDuplicates = AllowDuplicates,
            OrdoRegex = OrdoRegex,
            FamiliaRegex = FamiliaRegex,
            GenusRegex = GenusRegex,
            SpeciesRegex = SpeciesRegex,
        };

        return e;
    }

    public EventDto ToFullDto()
    {
        var e = new EventDto() {
            Id = Id,
            Name = Name,
            Start = Start,
            End = End,
            MainAdminId = MainAdminId,
            PublicIdentifier = PublicIdentifier,
            AllowDuplicates = AllowDuplicates,
            OrdoRegex = OrdoRegex,
            FamiliaRegex = FamiliaRegex,
            GenusRegex = GenusRegex,
            SpeciesRegex = SpeciesRegex,
        };

        if (MainAdmin is not null) e.MainAdmin = MainAdmin.ToDto();

        if (e.Participants is null) e.Participants = new List<WatcherDto>();
        foreach (var w in Participants)
            e.Participants.Add(w.ToDto());

        return e;
    }
}
