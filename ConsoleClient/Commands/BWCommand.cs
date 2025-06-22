using System.CommandLine;

namespace BirdWatching.ConsoleClient;

public class BWCommand : Command
{
    protected ImportantStuff _impStuff;

    public BWCommand(string name, string description, ImportantStuff impStuff) : base(name, description)
    {
        _impStuff = impStuff;
    }
}
