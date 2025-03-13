using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using BlazorCharts.UI.Services;
using Microsoft.Extensions.DependencyInjection;


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
            builder.Services.AddScoped(sp => new MapboxService(
    sp.GetRequiredService<HttpClient>(),
    "pk.eyJ1IjoicWZzZXJ2aWNlIiwiYSI6ImNtNXZlZXFuajAxMW4yanE2NXlkYzVjYjQifQ.yt3RAlNbu7pOWYH2imLdJA"
));

            await builder.Build().RunAsync();
        }
    }
}
