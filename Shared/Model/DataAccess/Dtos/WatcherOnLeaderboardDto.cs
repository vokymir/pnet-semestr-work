namespace BirdWatching.Shared.Model;

public class WatcherOnLeaderboardDto
{
    public int Id { get; set; }
    public string PublicIdentifier { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public List<RecordDto>? ValidRecords { get; set; } = new();
}
