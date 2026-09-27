namespace StreetWars.Backend.Game;

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

public static class GameSnapshotFactory
{
    private static readonly string[] TerritoryNames =
    [
        "Downtown", "Industrial", "Docks", "Old Highway", "Neon District"
    ];

    public static GameSnapshot Create(string sessionId, StreetWarsGame game)
    {
        game.IsGameOver(out var winner);

        var players = game.Players
            .OfType<StreetWarsPlayer>()
            .Select(player => new PlayerSnapshot(
                player.PlayerId,
                player.LifeValue,
                player.ManaValue,
                game.GetHand(player.PlayerId).Select(ToSnapshot).ToList()))
            .ToList();

        return new GameSnapshot(
            sessionId,
            (game.ActivePlayer as StreetWarsPlayer)?.PlayerId ?? string.Empty,
            winner,
            players,
            Enumerable.Range(0, StreetWarsGame.TerritoryCount)
                .Select(i => CreateTerritory(game, i))
                .ToList());
    }

    private static TerritorySnapshot CreateTerritory(StreetWarsGame game, int index)
    {
        var occupants = game.Players
            .OfType<StreetWarsPlayer>()
            .Select(player => (player.PlayerId, Card: game.GetCardAt(player, index)))
            .Where(x => x.Card is not null)
            .Select(x => new TerritoryOccupantSnapshot(x.PlayerId, ToSnapshot(x.Card!)))
            .ToList();

        var controller = DetermineController(occupants);

        return new TerritorySnapshot(
            index,
            TerritoryNames[index],
            occupants,
            controller);
    }

    private static string DetermineController(IReadOnlyList<TerritoryOccupantSnapshot> occupants)
    {
        if (occupants.Count == 0)
            return "Neutral";

        var ordered = occupants
            .OrderByDescending(x => x.Card.Attack + x.Card.Life)
            .ToList();

        if (ordered.Count > 1 &&
            ordered[0].Card.Attack + ordered[0].Card.Life ==
            ordered[1].Card.Attack + ordered[1].Card.Life)
        {
            return "Contested";
        }

        return ordered[0].PlayerId;
    }

    private static CardSnapshot ToSnapshot(StreetCarCard card) =>
        new(card.CarId, card.Model, card.AttackValue, card.LifeValue, card.ManaValue, card.IsReadyToAttack);
}
