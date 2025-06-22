using System.CommandLine;
using BirdWatching.Shared.Model;

namespace BirdWatching.ConsoleClient;

public class WatcherAddCommand : BWCommand
{
    public WatcherAddCommand(ImportantStuff stuff) :
        base("add", "Create new watcher.", stuff)
    {
        Add(_firstNameOption);
        Add(_lastNameOption);

        SetAction(
                async (parseResult) => {
                    string fn = parseResult.GetValue(_firstNameOption) ?? "Jméno";
                    string ln = parseResult.GetValue(_lastNameOption) ?? "Příjmení";

                    await HandleCommand(fn, ln);
                }
            );
    }

    private readonly Option<string> _firstNameOption = new("--first-name", "-fn") {
        Arity = ArgumentArity.ExactlyOne,
        Description = "Watcher's first name."
    };

    private readonly Option<string> _lastNameOption = new("--last-name", "-ln") {
        Arity = ArgumentArity.ExactlyOne,
        Description = "Watcher's last name."
    };

    private async Task HandleCommand(string firstName, string lastName)
    {
        WatcherDto wd = new() { FirstName = firstName, LastName = lastName };

        Console.WriteLine("Try request.");
        await _impStuff.BirdApiClient.Watcher_CreateWatcherAsync(wd);
        Console.WriteLine("Request done.");
    }
}
