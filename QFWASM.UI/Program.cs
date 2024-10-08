using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using QFWASM.UI.Services;
using Microsoft.Extensions.DependencyInjection;


namespace QFWASM.UI
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


            await builder.Build().RunAsync();
        }
    }
}
