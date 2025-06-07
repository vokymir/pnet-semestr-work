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
        if (string.IsNullOrEmpty(token))
            throw new ApplicationException("Cannot login...");

        // Call an [Authorize] endpoint
        await CallEndpointAsync__GET(client, token, "/api/event/all");   // e.g. returns events
                                                                         // Call an [Authorize(Roles="Admin")] endpoint
        await CallEndpointAsync__GET(client, token, "/api/user/all");    // Admin-only users

        WatcherDto w = new() { FirstName = "Jmeno", LastName = "Prijmeni" };
        await CreateWatcherAsync(client, token, w);

        var ws = await GetWatchersAsync(client, token);

        EventDto e = new() { Name = "Udalost", Start = DateTime.Now, End = DateTime.Now.AddDays(1) };
        await CreateEventAsync(client, token, e);

        var es = await GetEventsAsync(client, token);

        await JoinWatcherToEventAsync(client, token, ws[0].Id, es[es.Count - 1].PublicIdentifier);

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

    static async Task CreateWatcherAsync(HttpClient client, string token, WatcherDto watcher)
    {
        Console.WriteLine("\n📤 Creating new watcher...");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var json = JsonSerializer.Serialize(watcher);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/watcher", content);
        if (response.IsSuccessStatusCode)
        {
            string data = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"✅ Watcher created: {data}");
        }
        else
        {
            Console.WriteLine($"❌ Failed to create watcher: {response.StatusCode}");
        }
    }

    static async Task CreateEventAsync(HttpClient client, string token, EventDto evt)
    {
        Console.WriteLine("\n📤 Creating new event...");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var json = JsonSerializer.Serialize(evt);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/event", content);
        if (response.IsSuccessStatusCode)
        {
            string data = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"✅ Event created: {data}");
        }
        else
        {
            string error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ Failed to create event: {response.StatusCode}\n{error}");
        }
    }

    static async Task JoinWatcherToEventAsync(HttpClient client, string token, int watcherId, string eventPublicId)
    {
        Console.WriteLine($"\n📤 Joining watcher {watcherId} to event {eventPublicId}...");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var endpoint = $"/api/watcher/join/{eventPublicId}?watcherId={watcherId}";
        var emptyContent = new StringContent("", Encoding.UTF8, "application/json");

        var response = await client.PostAsync(endpoint, emptyContent);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("✅ Watcher successfully joined event.");
        }
        else
        {
            Console.WriteLine($"❌ Failed to join event: {response.StatusCode}");
        }
    }

    static async Task<List<WatcherDto>> GetWatchersAsync(HttpClient client, string token)
    {
        Console.WriteLine("\n📥 Retrieving your watchers...");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/watcher");
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"❌ Failed to retrieve watchers: {response.StatusCode}");
            return new List<WatcherDto>();
        }

        var stream = await response.Content.ReadAsStreamAsync();
        var watchers = await JsonSerializer.DeserializeAsync<List<WatcherDto>>(stream, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        });

        if (watchers is null || watchers.Count == 0)
        {
            Console.WriteLine("⚠️ No watchers found.");
            return new List<WatcherDto>();
        }

        foreach (var w in watchers)
            Console.WriteLine($"🕵️ {w.Id} - {w.FirstName} {w.LastName} ({w.PublicIdentifier})");

        return watchers;
    }

    static async Task<List<EventDto>> GetEventsAsync(HttpClient client, string token)
    {
        Console.WriteLine("\n📥 Retrieving all events...");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/event/all");
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"❌ Failed to retrieve events: {response.StatusCode}");
            return new List<EventDto>();
        }

        var stream = await response.Content.ReadAsStreamAsync();
        var events = await JsonSerializer.DeserializeAsync<List<EventDto>>(stream, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        });

        if (events is null || events.Count == 0)
        {
            Console.WriteLine("⚠️ No events found.");
            return new List<EventDto>();
        }

        foreach (var e in events)
            Console.WriteLine($"📅 {e.Id} - {e.Name} ({e.PublicIdentifier})");

        return events;
    }
}
