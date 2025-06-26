namespace BirdWatching.Shared.Model;

public class RecordDto : IAmDto<Record>
{
    public int Id { get; set; }
    public DateTime DateSeen { get; set; } = DateTime.Now;

    public string Comment { get; set; } = string.Empty;

    public int Count { get; set; } = 1;

    public int BirdId { get; set; }
    public BirdDto? Bird { get; set; }

    public int WatcherId { get; set; }
    public WatcherDto? Watcher { get; set; }

    public Record ToEntity()
    {
        var r = new Record() {
            Id = Id,
            DateSeen = DateSeen,
            Comment = Comment,
            Count = Count,
        };

        return r;
    }
}
