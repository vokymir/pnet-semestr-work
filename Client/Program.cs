using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using BirdWatching.Shared.Api;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddBlazorBootstrap();

        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddTransient<AuthHeaderHandler>();

        builder.Services.AddScoped<AuthHeaderHandler>();
        builder.Services.AddHttpClient<BirdApiClient>(client => {
            client.BaseAddress = new Uri("http://localhost:5069");
        }).AddHttpMessageHandler<AuthHeaderHandler>();

        await builder.Build().RunAsync();
    }
}
