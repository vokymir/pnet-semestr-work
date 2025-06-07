using System.Net.Http.Headers;
using BirdWatching.Shared.Model;
using BirdWatching.Shared.Api;

public class Program
{
    static HttpClient c = new HttpClient();
    static BirdApiClient client = new BirdApiClient("http://localhost:5069/", c);

    static async Task Main(string[] args)
    {
        var log = new LoginDto("string", "string");

        // 1. Authenticate
        string? token = await client.AuthAsync(log); // Assume it returns the JWT string
        if (token is null)
            throw new ApplicationException("Invalid token");

        // 2. Attach the token
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 3. Try calling the protected endpoints
        var bird = new BirdDto { Genus = "GEN", Species = "SPE" };

        var birds = await client.BirdAllAsync();
        PrintBirds(birds);

        await client.BirdPOSTAsync(bird);

        birds = await client.BirdAllAsync();
        PrintBirds(birds);
    }

    static void PrintBirds(ICollection<BirdDto>? birds)
    {
        if (birds is null)
        {
            Console.WriteLine("Birds is null...");
            return;
        }

        foreach (var b in birds)
            Console.WriteLine($"{b.Id}\t{b.Genus}\t{b.Species}");
    }
}
