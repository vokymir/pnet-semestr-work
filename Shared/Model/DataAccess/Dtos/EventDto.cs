namespace BirdWatching.Shared.Model;

public class EventDto : IAmDto<Event>
{
    public int Id { get; set; }
    public string PublicIdentifier { get; set; } = string.Empty;

    public string Name { get; set; } = "Nepojmenovany event";
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = false;

    public DateTime Start { get; set; } = DateTime.Now;
    public DateTime End { get; set; } = DateTime.MaxValue;
    public bool AllowDuplicates { get; set; } = false; // wheter to allow having the same bird twice in an event
    public string OrdoRegex { get; set; } = ".*";
    public string FamiliaRegex { get; set; } = ".*";
    public string GenusRegex { get; set; } = ".*";
    public string SpeciesRegex { get; set; } = ".*";

    public int MainAdminId { get; set; }
    public UserDto? MainAdmin { get; set; }
    public List<RecordDto> NotValidRecords { get; set; } = new();

    public ICollection<WatcherDto>? Participants { get; set; }

    public Event ToEntity()
    {
        var e = new Event() {
            Id = Id,
            Name = Name,
            Start = Start,
            End = End,
            PublicIdentifier = PublicIdentifier,
            AllowDuplicates = AllowDuplicates,
            OrdoRegex = OrdoRegex,
            FamiliaRegex = FamiliaRegex,
            GenusRegex = GenusRegex,
            SpeciesRegex = SpeciesRegex,
            Description = Description,
            IsPublic = IsPublic,
            MainAdminId = MainAdminId,
        };

        return e;
    }
}
