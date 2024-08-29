using Quantaflare.UI.Client.Pages;
using Quantaflare.UI.Services;
using Quantaflare.UI.Components;
using MudBlazor.Services;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents()
        .AddInteractiveWebAssemblyComponents();

    builder.Services.AddHttpClient<IUConnectServices, UConnectServices>
        (client =>
        {
            client.BaseAddress = new Uri("http://localhost:5221/");
        }
        );
    builder.Services.AddHttpClient<IEnergyClass, EnergyClass>
        (client =>
        {
            client.BaseAddress = new Uri("http://localhost:5221/");
        }
        );
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5221/") });
    
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddMudServices();
    var app = builder.Build();

    app.UseRouting();   // Should be before UseSession
    app.UseSession();   // Enable session middleware
    

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Quantaflare.UI.Client._Imports).Assembly);

app.Run();
}
catch (AggregateException ex)
{
    foreach (var innerException in ex.InnerExceptions)
    {
        Console.WriteLine("Exception during session setup: " + innerException.Message);
        // You can log or handle these exceptions accordingly
    }
}