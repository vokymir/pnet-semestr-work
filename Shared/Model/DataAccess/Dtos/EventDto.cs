namespace BirdWatching.Shared.Model;

public class EventDto : IAmDto<Event>
{
    public int Id { get; set; }
    public string Name { get; set; } = "Nepojmenovany event";
    public DateTime Start { get; set; } = DateTime.Now;
    public DateTime End { get; set; } = DateTime.MaxValue;
    public DateTime AddingDeadline { get; set; } = DateTime.MaxValue;

    public int MainAdminId { get; set; }
    public UserDto MainAdmin { get; set; } = null!;

    public ICollection<WatcherDto> Participants { get; set; } = new List<WatcherDto>();
    public ICollection<UserDto> Admins { get; set; } = new List<UserDto>();

    public Event ToEntity()
    {
        var e = new Event() {
            Id = Id,
            Name = Name,
            Start = Start,
            End = End,
            AddingDeadline = AddingDeadline,
            MainAdminId = MainAdminId,
            MainAdmin = MainAdmin.ToEntity()
        };

        foreach (var w in Participants)
            e.Participants.Add(w.ToEntity());
        foreach (var u in Admins)
            e.Admins.Add(u.ToEntity());

        return e;
    }
}
