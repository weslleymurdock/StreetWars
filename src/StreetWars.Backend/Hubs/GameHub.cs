using Microsoft.AspNetCore.SignalR;
using StreetWars.Backend.Game;
using StreetWars.Backend.Services;

namespace StreetWars.Backend.Hubs;

public sealed class GameHub(IGameSessionStore sessions, IStreetWarsAi ai) : Hub
{
    public async Task<GameSnapshot> CreateGame(bool withAi = false)
    {
        var session = sessions.Create(withAi);
        await Groups.AddToGroupAsync(Context.ConnectionId, session.Id);
        var snapshot = GameSnapshotFactory.Create(session.Id, session.Game);
        await Clients.Group(session.Id).SendAsync("GameStateChanged", snapshot);
        return snapshot;
    }

    public async Task<GameSnapshot> JoinGame(string sessionId)
    {
        var session = sessions.Get(sessionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, session.Id);
        return GameSnapshotFactory.Create(session.Id, session.Game);
    }

    public Task<GameSnapshot> PlayCar(string sessionId, string playerId, string cardId, int territory) =>
        Execute(sessionId, game => game.PlayCar(playerId, cardId, territory));

    public Task<GameSnapshot> Attack(string sessionId, string playerId, string attackerId, string targetId) =>
        Execute(sessionId, game => game.Attack(playerId, attackerId, targetId));

    public Task<GameSnapshot> EndTurn(string sessionId, string playerId) =>
        Execute(sessionId, game => game.EndTurn(playerId));

    private async Task<GameSnapshot> Execute(string sessionId, Action<StreetWarsGame> action)
    {
        var session = sessions.Get(sessionId);

        lock (session.Game)
        {
            action(session.Game);

            if (session.WithAi && session.Game.ActivePlayer == session.Game.StreetPlayerB)
                ai.PlayTurn(session.Game);
        }

        var snapshot = GameSnapshotFactory.Create(session.Id, session.Game);
        await Clients.Group(session.Id).SendAsync("GameStateChanged", snapshot);
        return snapshot;
    }
}
