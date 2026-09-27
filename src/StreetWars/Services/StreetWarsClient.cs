using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using StreetWars.Game;

namespace StreetWars.Services;

public interface IStreetWarsClient
{
    event Action<GameSnapshot>? StateChanged;
    Task<IReadOnlyList<RoomInfo>> GetRoomsAsync();
    Task<RoomAccess> CreateRoomAsync();
    Task<RoomAccess> CreateAiGameAsync();
    Task<RoomAccess> JoinRoomAsync(string sessionId);
    Task ConnectAsync(RoomAccess access);
    Task<GameSnapshot> GetStateAsync();
    Task<GameSnapshot> PlayCarAsync(string cardId, int territory);
    Task<GameSnapshot> AttackAsync(string attackerId, string targetId);
    Task<GameSnapshot> EndTurnAsync();
}

public sealed class StreetWarsClient : IStreetWarsClient, IAsyncDisposable
{
    private const string BackendUrl = "http://192.168.3.2:7000";

    private readonly HttpClient httpClient = new()
    {
        BaseAddress = new Uri(BackendUrl)
    };

    private HubConnection? connection;

    public event Action<GameSnapshot>? StateChanged;

    public async Task<IReadOnlyList<RoomInfo>> GetRoomsAsync() =>
        await httpClient.GetFromJsonAsync<List<RoomInfo>>("/api/rooms")
        ?? [];

    public Task<RoomAccess> CreateRoomAsync() =>
        CreateAccessAsync("/api/rooms");

    public Task<RoomAccess> CreateAiGameAsync() =>
        CreateAccessAsync("/api/rooms/ai");

    public async Task<RoomAccess> JoinRoomAsync(string sessionId)
    {
        using var response = await httpClient.PostAsync($"/api/rooms/{Uri.EscapeDataString(sessionId)}/join", null);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<RoomAccess>()
            ?? throw new InvalidOperationException("The backend did not return room credentials.");
    }

    public async Task ConnectAsync(RoomAccess access)
    {
        if (connection is not null)
        {
            await connection.DisposeAsync();
            connection = null;
        }

        connection = new HubConnectionBuilder()
            .WithUrl($"{BackendUrl}/hub/game", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(access.AccessToken);
            })
            .WithAutomaticReconnect()
            .Build();

        connection.On<GameSnapshot>("GameStateChanged", snapshot => StateChanged?.Invoke(snapshot));
        await connection.StartAsync();
    }

    public Task<GameSnapshot> GetStateAsync() =>
        InvokeAsync<GameSnapshot>("GetState");

    public Task<GameSnapshot> PlayCarAsync(string cardId, int territory) =>
        InvokeAsync<GameSnapshot>("PlayCar", cardId, territory);

    public Task<GameSnapshot> AttackAsync(string attackerId, string targetId) =>
        InvokeAsync<GameSnapshot>("Attack", attackerId, targetId);

    public Task<GameSnapshot> EndTurnAsync() =>
        InvokeAsync<GameSnapshot>("EndTurn");

    private async Task<RoomAccess> CreateAccessAsync(string endpoint)
    {
        using var response = await httpClient.PostAsync(endpoint, null);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<RoomAccess>()
            ?? throw new InvalidOperationException("The backend did not return room credentials.");
    }

    private async Task<T> InvokeAsync<T>(string method, params object?[] args)
    {
        if (connection is null)
            throw new InvalidOperationException("Join a room before sending game commands.");

        return await connection.InvokeAsync<T>(method, args);
    }

    public async ValueTask DisposeAsync()
    {
        if (connection is not null)
            await connection.DisposeAsync();

        httpClient.Dispose();
    }
}
