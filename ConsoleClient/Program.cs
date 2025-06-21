using BirdWatching.Shared.Api;

using System.CommandLine;

namespace BirdWatching.ConsoleClient;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var httpClient = new HttpClient() { BaseAddress = new Uri("http://localhost:5069") };
        var birdApiClient = new BirdApiClient(httpClient);

        var storedInfoPath = Path.Combine(".", ".info");

        var rootCommand = new RootCommand("A command-line client for the bird app.");
        var loginCommand = new LoginCommand(birdApiClient, storedInfoPath);

        rootCommand.Subcommands.Add(loginCommand);

        return await rootCommand.Parse(args).InvokeAsync();
    }
}

