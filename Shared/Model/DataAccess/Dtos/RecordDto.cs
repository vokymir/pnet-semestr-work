namespace BirdWatching.Shared.Model;

public class RecordDto : IAmDto<Record>
{
    public int Id { get; set; }
    public DateTime DateSeen { get; set; }

    public int BirdId { get; set; }
    public BirdDto Bird { get; set; } = null!;

    public int WatcherId { get; set; }

    public Record ToEntity()
    {
        var r = new Record() {
            Id = Id,
            DateSeen = DateSeen,
            BirdId = BirdId,
            WatcherId = WatcherId,
            Bird = Bird.ToEntity(),
        };

        return r;
    }
}
