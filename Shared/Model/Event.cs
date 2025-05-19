namespace BirdWatching.Shared.Model;

public class Event : IHaveDto<EventDto>
{
    public int Id { get; set; }
    public string Name { get; set; } = "Nepojmenovany event";
    public DateTime Start { get; set; } = DateTime.Now;
    public DateTime End { get; set; } = DateTime.MaxValue;
    public DateTime AddingDeadline { get; set; } = DateTime.MaxValue;

    public int MainAdminId { get; set; }
    public User MainAdmin { get; set; } = null!;

    public ICollection<Watcher> Participants { get; set; } = new List<Watcher>();
    public ICollection<User> Admins { get; set; } = new List<User>();

    public EventDto ToDto()
    {
        var e = new EventDto() {
            Id = Id,
            Name = Name,
            Start = Start,
            End = End,
            AddingDeadline = AddingDeadline,
            MainAdminId = MainAdminId,
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
            AddingDeadline = AddingDeadline,
            MainAdminId = MainAdminId,
            MainAdmin = MainAdmin.ToDto()
        };

        if (e.Participants is null) e.Participants = new List<WatcherDto>();
        foreach (var w in Participants)
            e.Participants.Add(w.ToDto());

        if (e.Admins is null) e.Admins = new List<UserDto>();
        foreach (var u in Admins)
            e.Admins.Add(u.ToDto());

        return e;
    }
}
