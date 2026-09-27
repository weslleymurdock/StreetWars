namespace StreetWars.Backend.Services;

public sealed record RoomResponse(string Id, int Players, int MaxPlayers);

public sealed record RoomAccessResponse(string SessionId, string PlayerId, string AccessToken);
