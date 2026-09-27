using Microsoft.AspNetCore.Authentication;
using StreetWars.Backend.Authentication;
using StreetWars.Backend.Game;
using StreetWars.Backend.Hubs;
using StreetWars.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("Room")
    .AddScheme<AuthenticationSchemeOptions, RoomAuthenticationHandler>("Room", _ => { });
builder.Services.AddAuthorization();

builder.Services.AddSignalR();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IGameSessionStore, GameSessionStore>();
builder.Services.AddSingleton<IPvpService, PvpService>();
builder.Services.AddSingleton<IStreetWarsAi, StreetWarsAi>();
builder.WebHost.UseUrls("http://0.0.0.0:7000");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(StreetWars.Backend.Common.Responses.Response<BackendGameProject>.Success(
    new BackendGameProject(name: "StreetWars.Backend"))))
.WithTags("Game");

app.MapGet("/api/rooms", (IGameSessionStore sessions) =>
    sessions.GetOpenRooms()
        .Select(room => new RoomResponse(room.Id, room.PlayerCount, room.MaxPlayers))
        .ToList());

app.MapPost("/api/rooms", (IGameSessionStore sessions) =>
{
    var session = sessions.Create(withAi: false);
    var access = sessions.CreatePlayerCredential(session, "A");
    return Results.Ok(new RoomAccessResponse(access.SessionId, access.PlayerId, access.AccessToken));
});

app.MapPost("/api/rooms/ai", (IGameSessionStore sessions) =>
{
    var session = sessions.Create(withAi: true);
    var access = sessions.CreatePlayerCredential(session, "A");
    return Results.Ok(new RoomAccessResponse(access.SessionId, access.PlayerId, access.AccessToken));
});

app.MapPost("/api/rooms/{sessionId}/join", (string sessionId, IGameSessionStore sessions) =>
{
    try
    {
        var access = sessions.Join(sessionId);
        return Results.Ok(new RoomAccessResponse(access.SessionId, access.PlayerId, access.AccessToken));
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
});

app.MapHub<GameHub>("/hub/game");

app.Run();
