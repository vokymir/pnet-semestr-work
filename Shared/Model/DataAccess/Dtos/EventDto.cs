namespace BirdWatching.Shared.Model;

public class EventDto : IAmDto<Event>
{
    public int Id { get; set; }
    public string PublicIdentifier { get; set; } = string.Empty;

    public string Name { get; set; } = "Nepojmenovany event";
    public DateTime Start { get; set; } = DateTime.Now;
    public DateTime End { get; set; } = DateTime.MaxValue;
    public DateTime AddingDeadline { get; set; } = DateTime.MaxValue;

    public int MainAdminId { get; set; }
    public UserDto? MainAdmin { get; set; }

    public ICollection<WatcherDto>? Participants { get; set; }
    public ICollection<UserDto>? Admins { get; set; }

    public Event ToEntity()
    {
        var e = new Event() {
            Id = Id,
            Name = Name,
            Start = Start,
            End = End,
            AddingDeadline = AddingDeadline,
            PublicIdentifier = PublicIdentifier
        };

        return e;
    }
}
