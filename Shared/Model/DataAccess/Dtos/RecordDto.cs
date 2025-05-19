namespace BirdWatching.Shared.Model;

public class RecordDto : IAmDto<Record>
{
    public int Id { get; set; }
    public DateTime DateSeen { get; set; }

    public int BirdId { get; set; }
    public BirdDto? Bird { get; set; }

    public int WatcherId { get; set; }
    public WatcherDto? Watcher { get; set; }

    public Record ToEntity()
    {
        var r = new Record() {
            Id = Id,
            DateSeen = DateSeen,
        };

        return r;
    }
}
