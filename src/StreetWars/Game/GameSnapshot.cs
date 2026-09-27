namespace StreetWars.Game;

public sealed record RoomInfo(
    string Id,
    int Players,
    int MaxPlayers);

public sealed record RoomAccess(
    string SessionId,
    string PlayerId,
    string AccessToken);

public sealed record GameSnapshot(
    string SessionId,
    string ActivePlayer,
    string? Winner,
    IReadOnlyList<PlayerSnapshot> Players,
    IReadOnlyList<TerritorySnapshot> Territories);

public sealed record PlayerSnapshot(
    string Id,
    int Life,
    int Mana,
    IReadOnlyList<CardSnapshot> Hand);

public sealed record CardSnapshot(
    string Id,
    string Model,
    int Attack,
    int Life,
    int Mana,
    bool ReadyToAttack);

public sealed record TerritorySnapshot(
    int Index,
    string Name,
    IReadOnlyList<TerritoryOccupantSnapshot> Occupants,
    string Controller);

public sealed record TerritoryOccupantSnapshot(
    string PlayerId,
    CardSnapshot Card);
