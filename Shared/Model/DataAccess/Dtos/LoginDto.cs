namespace BirdWatching.Shared.Model;

public record LoginDto(string username, string passwordhash, int WantedMinutes = 0);
