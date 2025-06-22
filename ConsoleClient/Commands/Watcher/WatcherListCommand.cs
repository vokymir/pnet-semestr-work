namespace BirdWatching.ConsoleClient;

public class WatcherListCommand : BWCommand
{
    public WatcherListCommand(ImportantStuff stuff) :
        base("list", "List all watchers you have access to.", stuff)
    {
        SetAction(
                async (parseResult) => await HandleCommand(stuff)
                );
    }

    private async Task HandleCommand(ImportantStuff stuff)
    {
        Console.WriteLine($"Attempting to get your watchers.");
        try
        {
            Console.ForegroundColor = _impStuff.Working.Foreground ?? Console.ForegroundColor;
            Console.BackgroundColor = _impStuff.Working.Background ?? Console.BackgroundColor;
            Console.WriteLine("Sending request to API...");
            Console.ResetColor();

            var response = await _impStuff.BirdApiClient.Watcher_GetUserWatchersAsync();
            if (response is null) throw new ApplicationException("Cannot retrieve info from the api...");

            Console.ForegroundColor = _impStuff.Success.Foreground ?? Console.ForegroundColor;
            Console.BackgroundColor = _impStuff.Success.Background ?? Console.BackgroundColor;
            Console.WriteLine("Request successful!");
            Console.ResetColor();

            foreach (var wD in response)
                Console.WriteLine($"{wD.Id}\t{wD.FirstName}\t{wD.LastName}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = _impStuff.Failure.Foreground ?? Console.ForegroundColor;
            Console.BackgroundColor = _impStuff.Failure.Background ?? Console.BackgroundColor;
            Console.Error.WriteLine($"Request failed: {ex.Message}");
            Console.ResetColor();
            Environment.Exit(1);
        }
    }
}
