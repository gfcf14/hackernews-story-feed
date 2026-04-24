using hackernews_api.Services;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(",") ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularFrontend", corsPolicyBuilder =>
    {
        corsPolicyBuilder.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add memory cache
builder.Services.AddMemoryCache();

// Add HttpClient with timeout
builder.Services.AddHttpClient<IHackerNewsClient, HackerNewsClient>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(10);
    });

// Add services
builder.Services.AddScoped<IStoryService, StoryService>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowAngularFrontend");
app.UseAuthorization();

// Map API routes
app.MapControllers();

// Only serve static files and fallback in production (when wwwroot exists with Angular files)
if (!app.Environment.IsDevelopment())
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
    // Fallback to index.html for Angular routing
    app.MapFallbackToFile("index.html");
}

app.Run();
