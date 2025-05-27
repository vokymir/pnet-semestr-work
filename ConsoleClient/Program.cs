using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BirdWatching.Shared.Model;

public class Program
{

    static HttpClient client = new HttpClient { BaseAddress = new Uri("http://localhost:5069/") };


    static async Task Main(string[] args)
    {
        string token = await LoginAsync(client, "string", "string");
        if (!string.IsNullOrEmpty(token))
        {
            // Call an [Authorize] endpoint
            await CallEndpointAsync__GET(client, token, "/api/event/all");   // e.g. returns events
                                                                             // Call an [Authorize(Roles="Admin")] endpoint
            await CallEndpointAsync__GET(client, token, "/api/user/all");    // Admin-only users
        }
    }

    static async Task<string> LoginAsync(HttpClient client, string username, string passwordHash)
    {
        // Construct the login data object
        var loginData = new { username = username, passwordhash = passwordHash };
        string json = JsonSerializer.Serialize(loginData);  // serialize to JSON:contentReference[oaicite:7]{index=7}
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Send POST to /api/auth
        HttpResponseMessage response = await client.PostAsync("/api/auth", content);
        if (response.IsSuccessStatusCode)
        {
            // Parse JSON response, e.g. { "token": "eyJ..." }
            string resultJson = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(resultJson);
            string token = doc.RootElement.GetProperty("token").GetString();
            return token;
        }
        else
        {
            Console.WriteLine($"Login failed: {response.StatusCode}");
            return null;
        }
    }


    static async Task CallEndpointAsync__GET(HttpClient client, string token, string endpoint)
    {
        // Attach the JWT in the Authorization header
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);  // Add Bearer token:contentReference[oaicite:9]{index=9}

        // Send GET request to the specified endpoint
        HttpResponseMessage response = await client.GetAsync(endpoint);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            Console.WriteLine($"401 Unauthorized when calling {endpoint}");
        }
        else if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            Console.WriteLine($"403 Forbidden when calling {endpoint}");
        }
        else if (response.IsSuccessStatusCode)
        {
            // Print the response body (e.g. list of events or users)
            string data = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Success ({endpoint}): {data}");
        }
        else
        {
            Console.WriteLine($"Error {(int) response.StatusCode} when calling {endpoint}");
        }
    }
}
