using BirdWatching.Shared.Api;
using BirdWatching.Shared.Model;

namespace BirdWatching.ConsoleClient;

public record ImportantStuff(
        BirdApiClient BirdApiClient,
        string StoredInfoPath,
        ConsoleStyle Success,
        ConsoleStyle Working,
        ConsoleStyle Failure
    );

public record ConsoleStyle(ConsoleColor? Foreground, ConsoleColor? Background);

public class StoredInfo
{
    public TokenResponseDto? TokenResponseDto { get; set; }
}

