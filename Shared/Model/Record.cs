namespace BirdWatching.Shared.Model;

public class Record : IHaveDto<RecordDto>
{
    public int Id { get; set; }
    public DateTime DateSeen { get; set; } = DateTime.Now;
    public string Comment { get; set; } = string.Empty;

    public int Count { get; set; } = 1; // how many same birds together - may be useful in some events

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Accuracy { get; set; }
    public string LocationDescribed { get; set; } = string.Empty;

    public int BirdId { get; set; }
    public Bird Bird { get; set; } = null!;

    public int WatcherId { get; set; }
    public Watcher Watcher { get; set; } = null!;

    public RecordDto ToDto()
    {
        var r = new RecordDto() {
            Id = Id,
            DateSeen = DateSeen,
            BirdId = BirdId,
            WatcherId = WatcherId,
            Comment = Comment,
            Count = Count,
            Latitude = Latitude,
            Longitude = Longitude,
            Accuracy = Accuracy,
            LocationDescribed = LocationDescribed,
        };

        if (Bird is not null)
            r.Bird = Bird.ToDto();

        return r;
    }

    public RecordDto ToFullDto()
    {
        var r = new RecordDto() {
            Id = Id,
            DateSeen = DateSeen,
            BirdId = BirdId,
            WatcherId = WatcherId,
            Comment = Comment,
            Count = Count,
            Latitude = Latitude,
            Longitude = Longitude,
            Accuracy = Accuracy,
            LocationDescribed = LocationDescribed,
        };

        if (Bird is not null)
            r.Bird = Bird.ToDto();

        if (Watcher is not null)
            r.Watcher = Watcher.ToDto();

        return r;
    }
}
