using System.Collections.Concurrent;
using System.Security.Cryptography;
using StreetWars.Backend.Game;

namespace StreetWars.Backend.Services;

public sealed record RoomInfo(string Id, int PlayerCount, int MaxPlayers);

public sealed record RoomAccess(string SessionId, string PlayerId, string AccessToken);

public interface IGameSessionStore
{
    GameSession Create(bool withAi);
    GameSession Get(string sessionId);
    IReadOnlyList<RoomInfo> GetOpenRooms();
    RoomAccess Join(string sessionId);
    bool TryAuthenticate(string accessToken, out string sessionId, out string playerId);
}

public sealed class GameSession
{
    public GameSession(string id, bool withAi, int maxPlayers)
    {
        Id = id;
        WithAi = withAi;
        MaxPlayers = maxPlayers;
        Game = StreetWarsGame.Create(withAi);
    }

    public string Id { get; }
    public bool WithAi { get; }
    public int MaxPlayers { get; }
    public StreetWarsGame Game { get; }
}

public sealed class GameSessionStore : IGameSessionStore
{
    public const int MaxPvpPlayers = 4;

    private readonly ConcurrentDictionary<string, GameSession> sessions = new();
    private readonly ConcurrentDictionary<string, (string SessionId, string PlayerId)> credentials = new();

    public GameSession Create(bool withAi)
    {
        var session = new GameSession(
            Guid.CreateVersion7().ToString("N"),
            withAi,
            withAi ? 2 : MaxPvpPlayers);

        sessions[session.Id] = session;
        return session;
    }

    public GameSession Get(string sessionId) =>
        sessions.TryGetValue(sessionId, out var session)
            ? session
            : throw new KeyNotFoundException($"Game session '{sessionId}' was not found.");

    public IReadOnlyList<RoomInfo> GetOpenRooms() =>
        sessions.Values
            .Where(session => !session.WithAi && session.Game.Players.Count < session.MaxPlayers)
            .OrderBy(session => session.Id)
            .Select(session => new RoomInfo(session.Id, session.Game.Players.Count, session.MaxPlayers))
            .ToList();

    public RoomAccess Join(string sessionId)
    {
        var session = Get(sessionId);

        lock (session.Game)
        {
            if (session.WithAi)
                throw new InvalidOperationException("AI games cannot be joined through the room list.");

            if (session.Game.Players.Count >= session.MaxPlayers)
                throw new InvalidOperationException("The room is full.");

            var playerId = session.Game.AddPlayer();
            return IssueCredential(session, playerId);
        }
    }

    public RoomAccess CreatePlayerCredential(GameSession session, string playerId) =>
        IssueCredential(session, playerId);

    public bool TryAuthenticate(string accessToken, out string sessionId, out string playerId)
    {
        if (credentials.TryGetValue(accessToken, out var credential))
        {
            sessionId = credential.SessionId;
            playerId = credential.PlayerId;
            return true;
        }

        sessionId = string.Empty;
        playerId = string.Empty;
        return false;
    }

    private RoomAccess IssueCredential(GameSession session, string playerId)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        credentials[token] = (session.Id, playerId);
        return new RoomAccess(session.Id, playerId, token);
    }
}
