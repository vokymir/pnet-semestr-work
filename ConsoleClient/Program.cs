using BirdWatching.Shared.Model;

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
        WatcherDto w = new WatcherDto() { FirstName = "Jarda", LastName = $"Sáček {DateTime.Now.ToString("yyyy-mm-dd HH:mm:ss")}" };
        await CreateNewWatcher(token, w);
        watchers = await ListUserWatchers(token);

        await ShowUserInfo(token);

        // create bird
        await ShowAllBirds();
        BirdDto b = new() { Genus = "Gen", Species = $"Spec {DateTime.Now.ToString("yyyy-mm-dd HH:mm:ss")}" };
        await NewBird(b);
        await ShowAllBirds();
        b = await GetBirdById(1);
        w = watchers[0];

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
        EventDto e = new() { MainAdminId = u.Id, Name = $"E: {DateTime.Now.ToString("yyyy-mm-dd HH:ss")}" };
        await CreateEvent(token, e);
        await ShowUserEvents(u.Id);
        // await JoinEvent(token, w, e);
        await ShowWatcherEvents(w);
        await ShowAllEvents();

        // add watcher to event
    }

    public static async Task<string> Login(UserDto user)
    {
        Console.WriteLine("START: Logging in...");
        string uri = Prefix + "Auth/Login";
        LoginDto l = new(user.UserName, user.PasswordHash);
        HttpResponseMessage msg = await client.PostAsJsonAsync(uri, l);

        if (msg.IsSuccessStatusCode)
        {
            string wtf = await msg.Content.ReadAsAsync<string>();
            Console.WriteLine(wtf + "\n=====");
            return wtf;
        }
        else
            return string.Empty;
    }

    public static async Task<List<WatcherDto>> ListUserWatchers(string token)
    {
        Console.WriteLine("START: Listing all watchers of one user...");
        string uri = $"{Prefix}Watcher/AllUserHave/{token}";
        var response = await client.GetAsync(uri);
        if (response.IsSuccessStatusCode)
        {
            var watchers = await response.Content.ReadAsAsync<List<WatcherDto>>();
            foreach (var w in watchers)
                Console.WriteLine($"{w.Id}\t{w.FirstName}\t{w.LastName}\t{w.PublicIdentifier}");
            Console.WriteLine("=====");
            return watchers;
        }
        return new List<WatcherDto>();
    }

    public static async Task CreateNewWatcher(string token, WatcherDto watcherInfo)
    {
        Console.WriteLine("START: Create new watcher...");
        string uri = $"{Prefix}Watcher/Create/{token}";
        var response = await client.PostAsJsonAsync(uri, watcherInfo);
    }

    public static async Task<UserDto> ShowUserInfo(string token)
    {
        Console.WriteLine("START: Show info about one user...");
        string uri = $"{Prefix}User/Get/{token}";
        var response = await client.GetAsync(uri);
        if (response.IsSuccessStatusCode)
        {
            var u = await response.Content.ReadAsAsync<UserDto>();
            if (u is null) Console.WriteLine($"VERY BAD, invalid user");
            else Console.WriteLine(
$"{u.Id} | {u.UserName}: {u.PasswordHash} | {(u.IsAdmin ? "Admin" : "Loser")} | W: {u.Watchers?.Count ?? -69} E: {u.Events?.Count ?? -69} | MainAdmin of W: {u.CuratedWatchers?.Count ?? -69} E: {u.AdministeredEvents?.Count ?? -69}\n=====");
            return u ?? new UserDto();
        }
        else
        {
            Console.WriteLine("Cannot show user info.");
            return new UserDto();
        }
    }

    public static async Task ShowAllBirds()
    {
        Console.WriteLine("START: Show all birds...");
        string uri = $"{Prefix}Bird/GetAll";
        var response = await client.GetAsync(uri);
        if (response.IsSuccessStatusCode)
        {
            var bs = await response.Content.ReadAsAsync<BirdDto[]>();
            if (bs is null) Console.WriteLine("No bird, simple null");
            else
                foreach (var b in bs)
                    Console.WriteLine($"{b.Id}\t{b.FullName}\t{b.Comment}");
            Console.WriteLine("=====");
        }
        else Console.WriteLine("NOTSUCCESSFULL get all birds\n=====");
    }

    public static async Task NewBird(BirdDto bdto)
    {
        Console.WriteLine("START: Create new bird...");
        string uri = $"{Prefix}Bird/Create";
        var response = await client.PostAsJsonAsync(uri, bdto);
        if (response.IsSuccessStatusCode) Console.WriteLine("Adding bird was a success.");
        else
        {
            var smth = await response.Content.ReadAsAsync<string>();
            Console.WriteLine($"Error when adding bird: {smth}");
        }
        Console.WriteLine("=====");
    }

    public static async Task<BirdDto> GetBirdById(int id)
    {
        Console.WriteLine("START: Get bird by id...");
        string uri = $"{Prefix}Bird/GetById/{id}";
        var response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Cannot get bird by id {id}: {await response.Content.ReadAsStringAsync()}\n=====");
            return new BirdDto() { Id = -69 };
        }
        var bird = await response.Content.ReadAsAsync<BirdDto>();
        Console.WriteLine("=====");
        return bird;
    }

    public static async Task ShowAllRecords()
    {
        Console.WriteLine("START: Show all records...");
        string uri = $"{Prefix}Record/GetAll";
        var response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode) Console.WriteLine($"Error fetching all records.");
        else
        {
            var rds = await response.Content.ReadAsAsync<RecordDto[]>();
            foreach (var rd in rds)
                Console.WriteLine($"{rd.Id}\tB:{rd.Bird?.FullName ?? "miss"}\tW:{rd.Watcher?.FirstName ?? "miss"} {rd.Watcher?.LastName ?? "miss"}\t{rd.DateSeen.ToString("yyyy-mm-dd HH:ss")}");
        }
        Console.WriteLine("=====");
    }

    public static async Task ShowWatcherRecords(int watcherId)
    {
        Console.WriteLine("START: Show all records of one watcher...");
        string uri = $"{Prefix}Record/GetByWatcher/{watcherId}";
        var response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode) Console.WriteLine($"Cannot get watchers' records, because: {response.StatusCode} : {response.RequestMessage} : {response.Content.ReadAsAsync<string>()}");
        else
        {
            var smth = await response.Content.ReadAsAsync<RecordDto[]>();
            if (smth is null) Console.WriteLine("ajaja, watchers records are null...");
            else
                foreach (var r in smth)
                    Console.WriteLine($"{r.Id}\tB:{r.BirdId}\tW:{r.WatcherId}\tD:{r.DateSeen}\tC:{r.Comment}");
        }
        Console.WriteLine("=====");
    }

    public static async Task AddRecordToWatcher(string token, RecordDto r)
    {
        Console.WriteLine("START: Add record to watcher...");
        string uri = $"{Prefix}Record/Create/{token}";
        var response = await client.PostAsJsonAsync(uri, r);

        if (!response.IsSuccessStatusCode) Console.WriteLine($"Cannot add record of birdId {r.BirdId} to watcherId {r.WatcherId}: {await response.Content.ReadAsStringAsync()}");
        Console.WriteLine("=====");
    }

    public static async Task ShowAllEvents()
    {
        Console.WriteLine("START: Show all events...");
        string uri = $"{Prefix}Event/GetAll";
        var response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode) Console.WriteLine($"Cannot get events");
        else
        {
            var smth = await response.Content.ReadAsAsync<EventDto[]>();
            if (smth is null) Console.WriteLine("ajaja, events are null...");
            else
                foreach (var e in smth)
                    Console.WriteLine($"{e.Id}\t{e.Name}\t#W:{e.Participants?.Count ?? -69}\tAdminId:{e.MainAdminId}");
        }
        Console.WriteLine("=====");
    }

    public static async Task ShowUserEvents(int id)
    {
        Console.WriteLine("START: Show all events belonging to user...");
        string uri = $"{Prefix}Event/GetByUserId/{id}";
        var response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode) Console.WriteLine("Cannot get events related to user.");
        else
        {
            var evs = await response.Content.ReadAsAsync<EventDto[]>();
            if (evs is null) Console.WriteLine("ajaj Events are null...");
            else
                foreach (var e in evs)
                    Console.WriteLine($"{e.Id}\t{e.Name}\t#W:{e.Participants?.Count ?? -69}\tAdminId:{e.MainAdminId}");
        }
        Console.WriteLine("=====");
    }

    public static async Task CreateEvent(string token, EventDto e)
    {
        Console.WriteLine("START: Create event...");
        string uri = $"{Prefix}Event/Create/{token}";
        var response = await client.PostAsJsonAsync(uri, e);

        if (!response.IsSuccessStatusCode) Console.WriteLine($"Cannot create event: {await response.Content.ReadAsStringAsync()}");
        Console.WriteLine("=====");
    }

    public static async Task ShowWatcherEvents(WatcherDto w)
    {
        Console.WriteLine("START: Show events in which watcher participates...");
        string uri = $"{Prefix}Event/GetByWatcherId/{w.Id}";
        var response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode) Console.WriteLine("Cannot show watchers events");
        else
            foreach (var e in await response.Content.ReadAsAsync<EventDto[]>())
                Console.WriteLine($"{e.Id}\t{e.Name}\t#W:{e.Participants?.Count ?? -69}\tAdminId:{e.MainAdminId}");

        Console.WriteLine("=====");
    }
}
