using StreetWars.Backend.Game;
using StreetWars.Backend.Hubs;
using StreetWars.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IGameSessionStore, GameSessionStore>();
builder.Services.AddSingleton<IStreetWarsAi, StreetWarsAi>();
builder.WebHost.UseUrls("http://0.0.0.0:7000");
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(StreetWars.Backend.Common.Responses.Response<BackendGameProject>.Success(
    new BackendGameProject(name: "StreetWars.Backend"))))
.WithTags("Game");

app.MapHub<GameHub>("/hub/game");

app.Run();
