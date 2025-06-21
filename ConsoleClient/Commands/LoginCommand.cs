namespace BirdWatching.ConsoleClient;

using System.CommandLine;
using System.Threading.Tasks;
using System.Text.Json;

using BirdWatching.Shared.Api;
using BirdWatching.Shared.Model;

public class LoginCommand : Command
{
    private readonly Option<string> _usernameOption = new("--username", "-u") {
        Required = true,
        Arity = ArgumentArity.ExactlyOne,
        Description = "The user's login username."
    };

    private readonly Option<string> _passwordOption = new("--password", "-p") {
        Required = true,
        Arity = ArgumentArity.ExactlyOne,
        Description = "The user's login password."
    };

    private readonly Option<int?> _validMinutesOption = new("--valid-minutes", "-m") {
        Arity = ArgumentArity.ZeroOrOne,
        Description = "Duration in minutes the session should be valid (optional)."
    };

    private readonly ImportantStuff _impStuff;

    public LoginCommand(ImportantStuff impStuff)
        : base("login", "Log in to the bird application.")
    {
        _impStuff = impStuff;

        Add(_usernameOption);
        Add(_passwordOption);
        Add(_validMinutesOption);

        SetAction(async (parseResult) => {
            string username = parseResult.GetValue(_usernameOption)!;
            string password = parseResult.GetValue(_passwordOption)!;
            int? validMinutes = parseResult.GetValue(_validMinutesOption);

            await HandleLoginCommand(username, password, validMinutes);
        });
    }

    private async Task HandleLoginCommand(
        string username,
        string password,
        int? validMinutes)
    {
        Console.WriteLine($"Attempting to log in with username: '{username}'");
        Console.WriteLine($"Password provided (masked): '********'");
        int minutes;

        if (validMinutes.HasValue)
        {
            Console.WriteLine($"Session will be valid for: {validMinutes.Value} minutes.");
            minutes = (int) validMinutes;
        }
        else
        {
            Console.WriteLine("Using default session validity period.");
            minutes = 0;
        }

        try
        {
            Console.ForegroundColor = _impStuff.Working.Foreground ?? Console.ForegroundColor;
            Console.BackgroundColor = _impStuff.Working.Background ?? Console.BackgroundColor;
            Console.WriteLine("Sending login request to API...");
            Console.ResetColor();

            var response = await _impStuff.BirdApiClient.Auth_LoginAsync(new LoginDto(username, password, minutes));
            if (response.Token is null) throw new ApplicationException("Cannot retrieve token from the api...");

            string infoJson = string.Empty;
            StoredInfo infoClass = new();

            // get preexisting data
            if (File.Exists(_impStuff.StoredInfoPath))
            {
                infoJson = await File.ReadAllTextAsync(_impStuff.StoredInfoPath, System.Text.Encoding.UTF8);
                infoClass = JsonSerializer.Deserialize<StoredInfo>(infoJson) ?? new();
            }

            // add new information
            infoClass.TokenResponseDto = response;

            // write back to file
            infoJson = JsonSerializer.Serialize(infoClass);
            await File.WriteAllTextAsync(_impStuff.StoredInfoPath, infoJson);

            Console.ForegroundColor = _impStuff.Success.Foreground ?? Console.ForegroundColor;
            Console.BackgroundColor = _impStuff.Success.Background ?? Console.BackgroundColor;
            Console.WriteLine("Login successful!");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = _impStuff.Failure.Foreground ?? Console.ForegroundColor;
            Console.BackgroundColor = _impStuff.Failure.Background ?? Console.BackgroundColor;
            Console.Error.WriteLine($"Login failed: {ex.Message}");
            Console.ResetColor();
            Environment.Exit(1);
        }
    }
}
