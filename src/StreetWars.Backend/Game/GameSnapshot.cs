namespace StreetWars.Backend.Game;

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

public static class GameSnapshotFactory
{
    private static readonly string[] TerritoryNames =
    [
        "Downtown", "Industrial", "Docks", "Old Highway", "Neon District"
    ];

    public static GameSnapshot Create(string sessionId, StreetWarsGame game)
    {
        game.IsGameOver(out var winner);

        return new GameSnapshot(
            sessionId,
            game.ActivePlayer == game.StreetPlayerA ? "A" : "B",
            winner,
            game.StreetPlayerA.LifeValue,
            game.StreetPlayerB.LifeValue,
            game.StreetPlayerA.ManaValue,
            game.StreetPlayerB.ManaValue,
            game.GetHand("A").Select(ToSnapshot).ToList(),
            game.GetHand("B").Select(ToSnapshot).ToList(),
            Enumerable.Range(0, StreetWarsGame.TerritoryCount)
                .Select(i => CreateTerritory(game, i))
                .ToList());
    }

    private static TerritorySnapshot CreateTerritory(StreetWarsGame game, int index)
    {
        var a = game.GetCardAt(game.StreetPlayerA, index);
        var b = game.GetCardAt(game.StreetPlayerB, index);

        var controller = a is null && b is null ? "Neutral"
            : a is not null && b is null ? "A"
            : b is not null && a is null ? "B"
            : Power(a!) > Power(b!) ? "A"
            : Power(b!) > Power(a!) ? "B"
            : "Contested";

        return new TerritorySnapshot(
            index,
            TerritoryNames[index],
            a is null ? null : ToSnapshot(a),
            b is null ? null : ToSnapshot(b),
            controller);
    }

    private static int Power(StreetCarCard card) => card.AttackValue + card.LifeValue;

    private static CardSnapshot ToSnapshot(StreetCarCard card) =>
        new(card.CarId, card.Model, card.AttackValue, card.LifeValue, card.ManaValue, card.IsReadyToAttack);
}
