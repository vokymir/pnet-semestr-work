using BirdWatching.Shared.Api;

using System.CommandLine;

namespace BirdWatching.ConsoleClient;

public class BirdCLI
{
    public static async Task<int> Main(string[] args)
    {
        var httpClient = new HttpClient() { BaseAddress = new Uri("http://localhost:5069") };
        var birdApiClient = new BirdApiClient(httpClient);

        var storedInfoPath = Path.Combine(".", ".info");

        ImportantStuff stuff = new(
                birdApiClient,
                storedInfoPath,
                new ConsoleStyle(ConsoleColor.Green, null),
                new ConsoleStyle(ConsoleColor.Yellow, null),
                new ConsoleStyle(ConsoleColor.Red, null)
        );

        var rootCommand = new RootCommand("A command-line client for the bird app.");
        var loginCommand = new LoginCommand(stuff);

        rootCommand.Subcommands.Add(loginCommand);

        return await rootCommand.Parse(args).InvokeAsync();
    }
}

