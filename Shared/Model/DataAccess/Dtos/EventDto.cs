namespace BirdWatching.Shared.Model;

public class EventDto : IAmDto<Event>
{
    public int Id { get; set; }
    public string PublicIdentifier { get; set; } = string.Empty;

    public string Name { get; set; } = "Nepojmenovany event";
    public DateTime Start { get; set; } = DateTime.Now;
    public DateTime End { get; set; } = DateTime.MaxValue;
    public bool AllowDuplicates { get; set; } = false; // wheter to allow having the same bird twice in an event
    public string GenusRegex { get; set; } = "*";
    public string SpeciesRegex { get; set; } = "*";

    public int MainAdminId { get; set; }
    public UserDto? MainAdmin { get; set; }

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
            GenusRegex = GenusRegex,
            SpeciesRegex = SpeciesRegex,
        };

        return e;
    }
}
