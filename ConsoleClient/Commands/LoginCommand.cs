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

    private readonly BirdApiClient _birdApiClient;
    private readonly string _storedInfoPath;

    public LoginCommand(BirdApiClient birdApiClient, string storedInfoPath)
        : base("login", "Log in to the bird application.")
    {
        _birdApiClient = birdApiClient;
        _storedInfoPath = storedInfoPath;

        Add(_usernameOption);
        Add(_passwordOption);
        Add(_validMinutesOption);

        SetAction(async (parseResult) => {
            string username = parseResult.GetValue(_usernameOption)!;
            string password = parseResult.GetValue(_passwordOption)!;
            int? validMinutes = parseResult.GetValue(_validMinutesOption);

            await HandleLoginCommand(username, password, validMinutes, _birdApiClient);
        });
    }

    private async Task HandleLoginCommand(
        string username,
        string password,
        int? validMinutes,
        BirdApiClient birdApiClient)
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
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Sending login request to API...");
            Console.ResetColor();

            var response = await birdApiClient.Auth_LoginAsync(new LoginDto(username, password, minutes));
            if (response.Token is null) throw new ApplicationException("Cannot retrieve token from the api...");

            var text = JsonSerializer.Serialize(response);
            await File.WriteAllTextAsync(_storedInfoPath, text);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Login successful!");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Login failed: {ex.Message}");
            Console.ResetColor();
            Environment.Exit(1);
        }
    }
}
