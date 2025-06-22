using BirdWatching.Shared.Model;

namespace BirdWatching.ConsoleClient;

public class WatcherListCommand : BWCommand
{
    private WatcherDto[]? _data = null;

    public WatcherListCommand(ImportantStuff stuff) :
        base("list", "List all watchers you have access to.", stuff)
    {
        SetAction(async (parseResult) => await HandleCommandWrapper(HandleCommand));
    }

    private async Task<Delegate> HandleCommand()
    {
        var data = await _impStuff.BirdApiClient.Watcher_GetUserWatchersAsync();
        if (data is null) throw new ApplicationException("Cannot retrieve info from the api...");
        _data = data.ToArray();

        return () => {
            foreach (var wD in _data)
                Console.WriteLine($"{wD.Id}\t{wD.FirstName}\t{wD.LastName}");
        };
    }
}
