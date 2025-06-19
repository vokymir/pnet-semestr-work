namespace BirdWatching.Shared.Model;

public class Record : IHaveDto<RecordDto>
{
    public int Id { get; set; }
    public DateTime DateSeen { get; set; } = DateTime.Now;
    public string Comment { get; set; } = string.Empty;

    public float Lon { get; set; }
    public float Lat { get; set; }

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
        };

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
            Bird = Bird.ToDto(),
            Watcher = Watcher.ToDto()
        };

        return r;
    }
}
