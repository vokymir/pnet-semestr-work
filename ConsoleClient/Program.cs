using BirdWatching.Shared.Model;
using System.Text;
using System.Text.Json;

public class Program
{
    static readonly HttpClient client = new HttpClient();
    static readonly string Prefix = "http://localhost:5069/api/";

    static async Task Main()
    {
        // login user
        UserDto u = new() { UserName = "string", PasswordHash = "string" };
        string token = await Login(u);
        u = await ShowUserInfo(token);

        // create watcher
        List<WatcherDto> watchers = await ListUserWatchers(token);
        WatcherDto w = new WatcherDto() { FirstName = "Jarda", LastName = $"Sáček {DateTime.Now:yyyy-MM-dd HH:mm:ss}" };
        await CreateNewWatcher(token, w);
        watchers = await ListUserWatchers(token);

        await ShowUserInfo(token);

        // create bird
        await ShowAllBirds();
        BirdDto b = new() { Genus = "Gen", Species = $"Spec {DateTime.Now:yyyy-MM-dd HH:mm:ss}" };
        await NewBird(b);
        await ShowAllBirds();
        b = await GetBirdById(1);
        w = watchers.FirstOrDefault() ?? w;

        // create record
        await ShowAllRecords();
        await ShowWatcherRecords(w.Id);
        RecordDto r = new() { WatcherId = w.Id, BirdId = b.Id };
        await AddRecordToWatcher(token, r);
        await ShowWatcherRecords(w.Id);
        await ShowAllRecords();

        // create event
        await ShowAllEvents();
        await ShowUserEvents(u.Id);
        await ShowWatcherEvents(w);
        EventDto e = new() { MainAdminId = u.Id, Name = $"E: {DateTime.Now:yyyy-MM-dd HH:mm:ss}" };
        await CreateEvent(token, e);
        e = await GetEventById(new Random().Next(20));
        await ShowUserEvents(u.Id);

        // add watcher to event
        await JoinEvent(token, w, e);
        await ShowWatcherEvents(w);
        await ShowAllEvents();
    }

    // --- HELPERS ---

    private static async Task<T?> SafeReadAsync<T>(HttpContent content)
    {
        try
        {
            var stream = await content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deserializing response: {ex.Message}");
            return default;
        }
    }

    private static void LogHeader(string msg) => Console.WriteLine($"\n=== {msg} ===");

    // --- API METHODS ---

    public static async Task<string> Login(UserDto user)
    {
        LogHeader("Logging in");
        string uri = Prefix + "Auth/Login";
        var loginDto = new LoginDto(user.UserName, user.PasswordHash);

        HttpResponseMessage msg = await client.PostAsJsonAsync(uri, loginDto);
        if (msg.IsSuccessStatusCode)
        {
            string token = await msg.Content.ReadAsStringAsync();
            Console.WriteLine($"Token received: {token}");
            return token;
        }
        else
        {
            Console.WriteLine($"Login failed: {msg.StatusCode}");
            return string.Empty;
        }
    }

    public static async Task<List<WatcherDto>> ListUserWatchers(string token)
    {
        LogHeader("Listing watchers");
        string uri = $"{Prefix}Watcher/AllUserHave/{token}";
        var response = await client.GetAsync(uri);

        if (response.IsSuccessStatusCode)
        {
            var watchers = await SafeReadAsync<List<WatcherDto>>(response.Content);
            if (watchers != null)
            {
                foreach (var w in watchers)
                    Console.WriteLine($"{w.Id}\t{w.FirstName}\t{w.LastName}\t{w.PublicIdentifier}");
                return watchers;
            }
        }
        Console.WriteLine("No watchers found or error.");
        return new List<WatcherDto>();
    }

    public static async Task CreateNewWatcher(string token, WatcherDto watcherInfo)
    {
        LogHeader("Creating new watcher");
        string uri = $"{Prefix}Watcher/Create/{token}";
        var response = await client.PostAsJsonAsync(uri, watcherInfo);
        if (!response.IsSuccessStatusCode)
            Console.WriteLine($"Failed to create watcher: {response.StatusCode}");
    }

    public static async Task<UserDto> ShowUserInfo(string token)
    {
        LogHeader("Showing user info");
        string uri = $"{Prefix}User/Get/{token}";
        var response = await client.GetAsync(uri);

        if (response.IsSuccessStatusCode)
        {
            var user = await SafeReadAsync<UserDto>(response.Content);
            if (user == null)
            {
                Console.WriteLine("Invalid user data.");
                return new UserDto();
            }
            Console.WriteLine($"{user.Id} | {user.UserName} | {(user.IsAdmin ? "Admin" : "User")} | Watchers: {user.Watchers?.Count ?? 0} | Events: {user.Events?.Count ?? 0}");
            return user;
        }
        else
        {
            Console.WriteLine($"Failed to get user info: {response.StatusCode}");
            return new UserDto();
        }
    }

    public static async Task ShowAllBirds()
    {
        LogHeader("Showing all birds");
        string uri = $"{Prefix}Bird/GetAll";
        var response = await client.GetAsync(uri);
        if (response.IsSuccessStatusCode)
        {
            var birds = await SafeReadAsync<BirdDto[]>(response.Content);
            if (birds == null || birds.Length == 0)
            {
                Console.WriteLine("No birds found.");
            }
            else
            {
                foreach (var b in birds)
                    Console.WriteLine($"{b.Id}\t{b.FullName}\t{b.Comment}");
            }
        }
        else
        {
            Console.WriteLine($"Failed to get birds: {response.StatusCode}");
        }
    }

    public static async Task NewBird(BirdDto bdto)
    {
        LogHeader("Creating new bird");
        string uri = $"{Prefix}Bird/Create";
        var response = await client.PostAsJsonAsync(uri, bdto);
        if (response.IsSuccessStatusCode)
            Console.WriteLine("Bird added successfully.");
        else
        {
            string error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error adding bird: {error}");
        }
    }

    public static async Task<BirdDto> GetBirdById(int id)
    {
        LogHeader($"Getting bird by id {id}");
        string uri = $"{Prefix}Bird/GetById/{id}";
        var response = await client.GetAsync(uri);
        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Failed to get bird: {error}");
            return new BirdDto { Id = -1 };
        }
        var bird = await SafeReadAsync<BirdDto>(response.Content);
        return bird ?? new BirdDto { Id = -1 };
    }

    public static async Task ShowAllRecords()
    {
        LogHeader("Showing all records");
        string uri = $"{Prefix}Record/GetAll";
        var response = await client.GetAsync(uri);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to get records: {response.StatusCode}");
            return;
        }

        var records = await SafeReadAsync<RecordDto[]>(response.Content);
        if (records != null)
        {
            foreach (var r in records)
                Console.WriteLine($"{r.Id}\tB:{r.Bird?.FullName ?? "N/A"}\tW:{r.Watcher?.FirstName ?? "N/A"} {r.Watcher?.LastName ?? ""}\t{r.DateSeen:yyyy-MM-dd HH:mm}");
        }
        else
        {
            Console.WriteLine("No records found.");
        }
    }

    public static async Task ShowWatcherRecords(int watcherId)
    {
        LogHeader($"Showing records for watcher {watcherId}");
        string uri = $"{Prefix}Record/GetByWatcher/{watcherId}";
        var response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to get watcher records: {response.StatusCode}");
            return;
        }

        var records = await SafeReadAsync<RecordDto[]>(response.Content);
        if (records == null)
        {
            Console.WriteLine("No records found for watcher.");
            return;
        }

        foreach (var r in records)
            Console.WriteLine($"{r.Id}\tB:{r.BirdId}\tW:{r.WatcherId}\tD:{r.DateSeen:yyyy-MM-dd HH:mm}\tC:{r.Comment}");
    }

    public static async Task AddRecordToWatcher(string token, RecordDto record)
    {
        LogHeader("Adding record to watcher");
        string uri = $"{Prefix}Record/Create/{token}";
        var response = await client.PostAsJsonAsync(uri, record);
        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Failed to add record: {error}");
        }
    }

    public static async Task ShowAllEvents()
    {
        LogHeader("Showing all events");
        string uri = $"{Prefix}Event/GetAll";
        var response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to get events: {response.StatusCode}");
            return;
        }

        var events = await SafeReadAsync<EventDto[]>(response.Content);
        if (events == null)
        {
            Console.WriteLine("No events found.");
            return;
        }

        foreach (var e in events)
            Console.WriteLine($"{e.Id}\t{e.Name}\tParticipants: {e.Participants?.Count ?? 0}\tAdminId: {e.MainAdminId}\t{e.PublicIdentifier}");
    }

    public static async Task ShowUserEvents(int userId)
    {
        LogHeader($"Showing events for user {userId}");
        string uri = $"{Prefix}Event/GetByUserId/{userId}";
        var response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to get user events: {response.StatusCode}");
            return;
        }

        var events = await SafeReadAsync<EventDto[]>(response.Content);
        if (events == null)
        {
            Console.WriteLine("No events found for user.");
            return;
        }

        foreach (var e in events)
            Console.WriteLine($"{e.Id}\t{e.Name}\tParticipants: {e.Participants?.Count ?? 0}\tAdminId: {e.MainAdminId}\t{e.PublicIdentifier}");
    }

    public static async Task CreateEvent(string token, EventDto e)
    {
        LogHeader("Creating event");
        string uri = $"{Prefix}Event/Create/{token}";
        var response = await client.PostAsJsonAsync(uri, e);
        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Failed to create event: {error}");
        }
    }

    public static async Task ShowWatcherEvents(WatcherDto w)
    {
        LogHeader($"Showing events for watcher {w.Id}");
        string uri = $"{Prefix}Event/GetByWatcherId/{w.Id}";
        var response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Failed to get watcher events.");
            return;
        }

        var events = await SafeReadAsync<EventDto[]>(response.Content);
        if (events == null)
        {
            Console.WriteLine("No events found for watcher.");
            return;
        }

        foreach (var e in events)
            Console.WriteLine($"{e.Id}\t{e.Name}\tParticipants: {e.Participants?.Count ?? 0}\tAdminId: {e.MainAdminId}");
    }

    public static async Task<EventDto> GetEventById(int id)
    {
        LogHeader($"Getting event by id {id}");
        string uri = $"{Prefix}Event/Get/{id}";
        var response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Failed to get event: {error}");
            return new EventDto { Name = "NENE" };
        }

        var e = await SafeReadAsync<EventDto>(response.Content);
        return e ?? new EventDto { Name = "NENE" };
    }

    public static async Task JoinEvent(string token, WatcherDto w, EventDto e)
    {
        LogHeader($"Joining event {e.PublicIdentifier}");
        string uri = $"{Prefix}Watcher/JoinEvent/{token}/{w.Id}/{e.PublicIdentifier}";
        var emptyContent = new StringContent("", Encoding.UTF8, "application/json");
        var response = await client.PostAsync(uri, emptyContent);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to join event: {response.StatusCode}");
        }
    }
}
