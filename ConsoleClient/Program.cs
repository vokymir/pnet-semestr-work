using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BirdWatching.Shared.Model;
using Newtonsoft.Json;

public class Program
{

    static readonly HttpClient client = new HttpClient();
    static readonly string Prefix = "http://localhost:5069/api/";

    static async Task Main()
    {
        User user = new User() { UserName = "string", PasswordHash = "string" };
        string token = await Login(user);

        List<WatcherDto> watchers = await ListUserWatchers(token);

        WatcherDto watcher = new WatcherDto() { FirstName = "Jarda", LastName = $"Sáček {DateTime.Now.ToString("yyyy-mm-dd HH:mm:ss")}" };
        await CreateNewWatcher(token, watcher);
        watchers = await ListUserWatchers(token);

        await ShowUserInfo(token);
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
                Console.WriteLine($"{w.Id} | {w.FirstName} {w.LastName}");
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
            Console.WriteLine($"{u.Id} | {u.UserName}: {u.PasswordHash} | {(u.IsAdmin ? "Admin" : "Loser")} | W: {u.Watchers.Count} E: {u.Events.Count} | MainAdmin of W: {u.CuratedWatchers.Count} E: {u.AdministeredEvents.Count}");
        }
        else
        {
            Console.WriteLine("Cannot show user info.");
        }
    }
}
