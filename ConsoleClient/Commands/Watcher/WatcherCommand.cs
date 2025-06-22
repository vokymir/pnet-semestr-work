namespace BirdWatching.ConsoleClient;

public class WatcherCommand : BWCommand
{
    public WatcherCommand(ImportantStuff stuff) :
        base("watcher", "All it is possible to do with watchers.", stuff)
    { }
}
