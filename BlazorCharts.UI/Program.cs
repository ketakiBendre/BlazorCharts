using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using BlazorCharts.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using static System.Net.WebRequestMethods;
using System.Net.Http.Json;
using BlazorCharts.Data;



namespace BlazorCharts.UI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");
            

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5221/") });
            builder.Services.AddHttpClient<MyChartService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5221/"); // Set your API base address
            });

            builder.Services.AddMudServices();
            var http = new HttpClient { BaseAddress = new Uri("https://localhost:7036/") };
            var config = await http.GetFromJsonAsync<AppConfig>("appsettings.json");

            if (config == null)
            {
                throw new Exception("Failed to load appsettings.json");
            }
            builder.Services.AddScoped(sp => new MapboxService(sp.GetRequiredService<HttpClient>(), config.Mapbox.AccessToken));
            // Register config as a service
            builder.Services.AddSingleton(config);

            await builder.Build().RunAsync();
        }
    }
}
