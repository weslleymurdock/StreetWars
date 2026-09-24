using System.Collections.Concurrent;
using StreetWars.Backend.Game;

namespace StreetWars.Backend.Services;

public interface IGameSessionStore
{
    GameSession Create(bool withAi);
    GameSession Get(string sessionId);
}

public sealed class GameSession
{
    public GameSession(string id, bool withAi)
    {
        Id = id;
        WithAi = withAi;
        Game = StreetWarsGame.Create();
    }

    public string Id { get; }
    public bool WithAi { get; }
    public StreetWarsGame Game { get; }
}

public sealed class GameSessionStore : IGameSessionStore
{
    private readonly ConcurrentDictionary<string, GameSession> sessions = new();

    public GameSession Create(bool withAi)
    {
        var session = new GameSession(Guid.CreateVersion7().ToString("N"), withAi);
        sessions[session.Id] = session;
        return session;
    }

    public GameSession Get(string sessionId) =>
        sessions.TryGetValue(sessionId, out var session)
            ? session
            : throw new KeyNotFoundException($"Game session '{sessionId}' was not found.");
}
