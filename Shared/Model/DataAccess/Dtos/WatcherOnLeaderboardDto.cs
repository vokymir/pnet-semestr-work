namespace BirdWatching.Shared.Model;

public class WatcherOnLeaderboardDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<RecordDto>? ValidRecords { get; set; } = new();
}
