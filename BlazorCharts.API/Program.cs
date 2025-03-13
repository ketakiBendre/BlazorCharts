var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddHttpClient("ApiClient", c =>
{
    c.BaseAddress = new Uri("http://localhost:5221/");
});


builder.Services.AddCors(p =>
    p.AddPolicy("APIPOL", builder =>
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.UseRouting();   // Should be before UseSession
app.UseSession();   // Enable session middleware
app.MapControllers();
app.UseCors("APIPOL");
app.UseHttpsRedirection();

app.Run();
