namespace StreetWars.Game;

public sealed record GameSnapshot(
    string SessionId,
    string ActivePlayer,
    string? Winner,
    int PlayerALife,
    int PlayerBLife,
    int PlayerAMana,
    int PlayerBMana,
    IReadOnlyList<CardSnapshot> PlayerAHand,
    IReadOnlyList<CardSnapshot> PlayerBHand,
    IReadOnlyList<TerritorySnapshot> Territories);

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
    CardSnapshot? PlayerA,
    CardSnapshot? PlayerB,
    string Controller);
