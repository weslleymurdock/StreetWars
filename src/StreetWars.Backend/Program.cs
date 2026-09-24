using StreetWars.Backend.Hubs;
using StreetWars.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IGameSessionStore, GameSessionStore>();
builder.Services.AddSingleton<IStreetWarsAi, StreetWarsAi>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new
{
    name = "StreetWars.Backend",
    game = "StreetWars",
    transport = "SignalR",
    hub = "/gameHub"
}));

app.MapHub<GameHub>("/gameHub");

app.Run();
