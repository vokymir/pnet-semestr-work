using BirdWatching.Shared.Model;

public class Program
{

    static readonly HttpClient client = new HttpClient();
    static readonly string Prefix = "http://localhost:5069/api/";

    static async Task Main()
    {
        // login user
        User user = new User() { UserName = "string", PasswordHash = "string" };
        string token = await Login(user);
        await ShowUserInfo(token);

        // create watcher
        List<WatcherDto> watchers = await ListUserWatchers(token);
        WatcherDto watcher = new WatcherDto() { FirstName = "Jarda", LastName = $"Sáček {DateTime.Now.ToString("yyyy-mm-dd HH:mm:ss")}" };
        await CreateNewWatcher(token, watcher);
        watchers = await ListUserWatchers(token);

        await ShowUserInfo(token);

        // create bird
        await ShowAllBirds();
        BirdDto bird = new() { Genus = "Gen", Species = $"Spec {DateTime.Now.ToString("yyyy-mm-dd HH:mm:ss")}" };
        await NewBird(bird);
        await ShowAllBirds();

        // create record

        // create event

        // add watcher to event
    }

    public static async Task<string> Login(User user)
    {
        string uri = Prefix + "Auth/Login";
        HttpResponseMessage msg = await client.PostAsJsonAsync(uri, user);

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
        string uri = $"{Prefix}Watcher/Create/{token}";
        var response = await client.PostAsJsonAsync(uri, watcherInfo);
    }

    public static async Task ShowUserInfo(string token)
    {
        string uri = $"{Prefix}User/Get/{token}";
        var response = await client.GetAsync(uri);
        if (response.IsSuccessStatusCode)
        {
            var u = await response.Content.ReadAsAsync<UserDto>();
            if (u is null) Console.WriteLine($"VERY BAD, invalid user");
            else Console.WriteLine(
$"{u.Id} | {u.UserName}: {u.PasswordHash} | {(u.IsAdmin ? "Admin" : "Loser")} | W: {u.Watchers?.Count ?? -69} E: {u.Events?.Count ?? -69} | MainAdmin of W: {u.CuratedWatchers?.Count ?? -69} E: {u.AdministeredEvents?.Count ?? -69}\n=====");
        }
        else
        {
            Console.WriteLine("Cannot show user info.");
        }
    }

    public static async Task ShowAllBirds()
    {
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
        string uri = $"{Prefix}Bird/Create";
        var response = await client.PostAsJsonAsync(uri, bdto);
        if (response.IsSuccessStatusCode) Console.WriteLine("Adding bird was a success.");
        else
        {
            var smth = response.Content.ReadAsAsync<string>();
            Console.WriteLine($"Error when adding bird: {smth}");
        }
        Console.WriteLine("=====");
    }
}
