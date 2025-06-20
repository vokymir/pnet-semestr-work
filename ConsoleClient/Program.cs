using System.Net.Http.Headers;
using BirdWatching.Shared.Model;
using BirdWatching.Shared.Api;

public class Program
{
    static HttpClient c = new HttpClient() { BaseAddress = new Uri("http://localhost:5069/") };
    static BirdApiClient client = new BirdApiClient(c);

    static async Task Main(string[] args)
    {
        var log = new LoginDto("string", "string");

        // 1. Authenticate
        TokenResponseDto? token = await client.Auth_LoginAsync(log);
        if (token is null)
            throw new ApplicationException("Invalid token");

        // 2. Attach the token
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        // 3. Try calling the protected endpoints
        var bird = new BirdDto { Genus = "GEN", Species = "SPE" + DateTime.Now.ToString(" dd.MM.yyyy HH:mm:ss") };

        var birds = await client.Bird_GetAllAsync();
        PrintBirds(birds, "\n###\nBefore\n###");

        await client.Bird_CreateAsync(bird);

        birds = await client.Bird_GetAllAsync();
        PrintBirds(birds, "\n###\nAfter\n###");
    }

    static void PrintBirds(ICollection<BirdDto>? birds, string? message = null)
    {
        if (message is not null)
            Console.WriteLine(message);

        if (birds is null)
        {
            Console.WriteLine("Birds is null...");
            return;
        }

        foreach (var b in birds)
            Console.WriteLine($"{b.Id}\t{b.Genus}\t{b.Species}");
    }
}
