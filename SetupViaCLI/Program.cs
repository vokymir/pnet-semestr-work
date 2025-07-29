using Microsoft.Extensions.Configuration;
using BirdWatching.Shared.Api;
using BirdWatching.Shared.Model;

if (args.Length != 3)
{
    Console.WriteLine("Usage: SetupViaCLI <username> <password> <path-to-csv>");
    return;
}

var username = args[0];
var password = args[1];
var csvPath = args[2];

if (!File.Exists(csvPath))
{
    Console.WriteLine($"CSV file not found: {csvPath}");
    return;
}

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var baseUrl = config["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(baseUrl))
{
    Console.WriteLine("Missing ApiBaseUrl in config.");
    return;
}

HttpClient cli = new HttpClient {
    BaseAddress = new Uri(baseUrl)
};

var api = new BirdApiClient(cli);

var loginDto = new LoginDto {
    username = username,
    passwordhash = password
};

TokenResponseDto response = await api.Auth_LoginAsync(loginDto);
if (response == null || string.IsNullOrWhiteSpace(response.Token))
{
    Console.WriteLine("Login failed.");
    return;
}

cli.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", response.Token);

int count = 0;

foreach (var line in File.ReadLines(csvPath))
{
    var parts = line.Split(',', StringSplitOptions.TrimEntries);
    if (parts.Length != 4)
    {
        Console.WriteLine($"Invalid line: {line}");
        continue;
    }

    var bird = new BirdDto {
        Genus = parts[0],
        Species = parts[1],
        Familia = parts[2],
        Ordo = parts[3]
    };

    if (bird.Genus == "Genus")
        continue;

    if (await api.Bird_CreateAsync(bird) is BirdDto)
        count++;
    else
        Console.WriteLine($"Failed to insert bird: {line}");
}

Console.WriteLine($"Inserted {count} birds.");
