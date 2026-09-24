using Microsoft.AspNetCore.SignalR.Client;
using StreetWars.Game;

namespace StreetWars.Services;

public interface IStreetWarsClient
{
    event Action<GameSnapshot>? StateChanged;
    Task ConnectAsync();
    Task<GameSnapshot> CreateGameAsync(bool withAi);
    Task<GameSnapshot> JoinGameAsync(string sessionId);
    Task<GameSnapshot> PlayCarAsync(string sessionId, string playerId, string cardId, int territory);
    Task<GameSnapshot> EndTurnAsync(string sessionId, string playerId);
}

public sealed class StreetWarsClient : IStreetWarsClient, IAsyncDisposable
{
    private readonly HubConnection connection;
    private bool connected;

    public event Action<GameSnapshot>? StateChanged;

    public StreetWarsClient()
    {
        var url = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5062/gameHub"
            : "http://localhost:5062/gameHub";

        connection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect()
            .Build();

        connection.On<GameSnapshot>("GameStateChanged", snapshot => StateChanged?.Invoke(snapshot));
    }

    public async Task ConnectAsync()
    {
        if (connected)
            return;

        await connection.StartAsync();
        connected = true;
    }

    public async Task<GameSnapshot> CreateGameAsync(bool withAi)
    {
        await ConnectAsync();
        return await connection.InvokeAsync<GameSnapshot>("CreateGame", withAi);
    }

    public async Task<GameSnapshot> JoinGameAsync(string sessionId)
    {
        await ConnectAsync();
        return await connection.InvokeAsync<GameSnapshot>("JoinGame", sessionId);
    }

    public async Task<GameSnapshot> PlayCarAsync(string sessionId, string playerId, string cardId, int territory)
    {
        await ConnectAsync();
        return await connection.InvokeAsync<GameSnapshot>("PlayCar", sessionId, playerId, cardId, territory);
    }

    public async Task<GameSnapshot> EndTurnAsync(string sessionId, string playerId)
    {
        await ConnectAsync();
        return await connection.InvokeAsync<GameSnapshot>("EndTurn", sessionId, playerId);
    }

    public async ValueTask DisposeAsync() => await connection.DisposeAsync();
}
