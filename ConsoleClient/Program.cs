using BirdWatching.Shared.Api;

using System.CommandLine;
using System.Text.Json;
using System.Net.Http.Headers;

namespace BirdWatching.ConsoleClient;

public class BirdCLI
{
    public static async Task<int> Main(string[] args)
    {
        var httpClient = new HttpClient() { BaseAddress = new Uri("http://localhost:5069") };
        var birdApiClient = new BirdApiClient(httpClient);

        var storedInfoPath = Path.Combine(".", ".info");

        ImportantStuff stuff = new(
                httpClient,
                birdApiClient,
                storedInfoPath,
                new ConsoleStyle(ConsoleColor.Green, null),
                new ConsoleStyle(ConsoleColor.Yellow, null),
                new ConsoleStyle(ConsoleColor.Red, null)
        );

        LoadToken(stuff);

        var rootCommand = new RootCommand("A command-line client for the bird app.");
        var loginCommand = new LoginCommand(stuff);
        var watcherCommand = new WatcherCommand(stuff);
        {
            var watcherListCommand = new WatcherListCommand(stuff);
            watcherCommand.Subcommands.Add(watcherListCommand);
        }

        rootCommand.Subcommands.Add(loginCommand);
        rootCommand.Subcommands.Add(watcherCommand);

        return await rootCommand.Parse(args).InvokeAsync();
    }

    private static void LoadToken(ImportantStuff stuff)
    {
        var storedInfo = JsonSerializer.Deserialize<StoredInfo>(File.ReadAllText(stuff.StoredInfoPath));

        if (storedInfo is null)
            return;

        stuff.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", storedInfo.TokenResponseDto?.Token);
    }
}

