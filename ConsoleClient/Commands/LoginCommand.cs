namespace BirdWatching.ConsoleClient;

using System.CommandLine;
using System.Threading.Tasks;
using System.Text.Json;

using BirdWatching.Shared.Model;

public class LoginCommand : BWCommand
{
    public LoginCommand(ImportantStuff impStuff)
        : base("login", "Log in to the bird application.", impStuff)
    {
        Add(_usernameOption);
        Add(_passwordOption);
        Add(_validMinutesOption);

        SetAction(async (parseResult) => {
            string username = parseResult.GetValue(_usernameOption)!;
            string password = parseResult.GetValue(_passwordOption)!;
            int? validMinutes = parseResult.GetValue(_validMinutesOption);

            await HandleCommandWrapper(HandleCommand, username, password, validMinutes);
        });
    }

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

    private async Task HandleCommand(
        string username,
        string password,
        int? validMinutes)
    {
        int minutes;

        if (validMinutes.HasValue)
            minutes = (int) validMinutes;
        else
            minutes = 0;

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
    }
}
