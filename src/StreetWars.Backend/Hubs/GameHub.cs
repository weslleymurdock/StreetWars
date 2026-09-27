using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StreetWars.Backend.Game;
using StreetWars.Backend.Services;

namespace StreetWars.Backend.Hubs;

[Authorize]
public sealed class GameHub(IGameSessionStore sessions, IPvpService pvp, IStreetWarsAi ai) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var session = GetSession();
        await Groups.AddToGroupAsync(Context.ConnectionId, session.Id);
        await base.OnConnectedAsync();
    }

    public Task<GameSnapshot> GetState() =>
        Task.FromResult(CreateSnapshot());

    public async Task<GameSnapshot> PlayCar(string cardId, int territory)
    {
        var session = GetSession();

        lock (session.Game)
        {
            pvp.PlayCar(session.Game, PlayerId, cardId, territory);
            RunAiIfNeeded(session);
        }

        return await Broadcast(session);
    }

    public async Task<GameSnapshot> Attack(string attackerId, string targetId)
    {
        var session = GetSession();

        lock (session.Game)
        {
            pvp.Attack(session.Game, PlayerId, attackerId, targetId);
            RunAiIfNeeded(session);
        }

        return await Broadcast(session);
    }

    public async Task<GameSnapshot> EndTurn()
    {
        var session = GetSession();

        lock (session.Game)
        {
            pvp.EndTurn(session.Game, PlayerId);
            RunAiIfNeeded(session);
        }

        return await Broadcast(session);
    }

    private GameSession GetSession()
    {
        var sessionId = Context.User?.FindFirstValue("session_id")
            ?? throw new HubException("The room credential is missing.");

        return sessions.Get(sessionId);
    }

    private string PlayerId =>
        Context.User?.FindFirstValue("player_id")
        ?? throw new HubException("The player credential is missing.");

    private void RunAiIfNeeded(GameSession session)
    {
        if (session.WithAi && session.Game.ActivePlayer == session.Game.GetPlayer("B"))
            ai.PlayTurn(session.Game);
    }

    private async Task<GameSnapshot> Broadcast(GameSession session)
    {
        var snapshot = GameSnapshotFactory.Create(session.Id, session.Game);
        await Clients.Group(session.Id).SendAsync("GameStateChanged", snapshot);
        return snapshot;
    }

    private GameSnapshot CreateSnapshot()
    {
        var session = GetSession();
        return GameSnapshotFactory.Create(session.Id, session.Game);
    }
}
