namespace BirdWatching.Shared.Model;

public class Record : IHaveDto<RecordDto>
{
    public int Id { get; set; }
    public DateTime DateSeen { get; set; }

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
            Bird = Bird.ToDto(),
        };

        return r;
    }
}
