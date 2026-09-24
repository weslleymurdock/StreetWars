using CardGameEngine;

namespace StreetWars.Backend.Game;

public sealed class StreetWarsGame : Game
{
    public const int TerritoryCount = 5;
    public const int TerritoryVictoryCount = 3;
    public const int InitialLife = 20;
    public const int InitialHandSize = 4;

    private StreetWarsGame(List<IPlayer> players) : base(players) { }

    public StreetWarsPlayer StreetPlayerA => (StreetWarsPlayer)Players[0];
    public StreetWarsPlayer StreetPlayerB => (StreetWarsPlayer)Players[1];

    public static StreetWarsGame Create()
    {
        var playerA = new StreetWarsPlayer(new Deck());
        var playerB = new StreetWarsPlayer(new Deck());
        var game = new StreetWarsGame([playerA, playerB]) { ActivePlayer = playerA };

        BuildDeck(playerA);
        BuildDeck(playerB);
        game.StartGame(InitialHandSize, InitialLife);
        return game;
    }

    public StreetWarsPlayer GetPlayer(string playerId) =>
        playerId.Equals("A", StringComparison.OrdinalIgnoreCase) ? StreetPlayerA : StreetPlayerB;

    public bool IsGameOver(out string? winner)
    {
        winner = null;
        if (GetControlledTerritories(StreetPlayerA).Count >= TerritoryVictoryCount) winner = "A";
        else if (GetControlledTerritories(StreetPlayerB).Count >= TerritoryVictoryCount) winner = "B";
        else if (!StreetPlayerA.IsAlive) winner = "B";
        else if (!StreetPlayerB.IsAlive) winner = "A";
        return winner is not null;
    }

    public IReadOnlyList<int> GetControlledTerritories(IPlayer player)
    {
        var opponent = player == StreetPlayerA ? StreetPlayerB : StreetPlayerA;
        var result = new List<int>();

        for (var i = 0; i < TerritoryCount; i++)
        {
            var own = GetCardAt(player, i);
            var enemy = GetCardAt(opponent, i);

            if (own is not null && (enemy is null || Power(own) > Power(enemy)))
                result.Add(i);
        }

        return result;
    }

    public StreetCarCard? GetCardAt(IPlayer player, int territory) =>
        territory is >= 0 and < TerritoryCount ? player.Board[territory] as StreetCarCard : null;

    public void PlayCar(string playerId, string cardId, int territory)
    {
        ValidatePlayerTurn(playerId);

        if (territory is < 0 or >= TerritoryCount)
            throw new CardGameEngineException("Territory must be between 0 and 4.");

        var player = GetPlayer(playerId);
        var card = player.Hand.AllCards.OfType<StreetCarCard>()
            .FirstOrDefault(c => c.CarId.Equals(cardId, StringComparison.OrdinalIgnoreCase));

        if (card is null)
            throw new CardGameEngineException("Car card was not found in the player's hand.");

        player.CastMonster(this, card, territory);
    }

    public void Attack(string playerId, string attackerId, string targetId)
    {
        ValidatePlayerTurn(playerId);

        var player = GetPlayer(playerId);
        var opponent = player == StreetPlayerA ? StreetPlayerB : StreetPlayerA;
        var attacker = player.Board.AllCards.OfType<StreetCarCard>()
            .FirstOrDefault(c => c.CarId.Equals(attackerId, StringComparison.OrdinalIgnoreCase));
        var target = opponent.Board.AllCards.OfType<StreetCarCard>()
            .FirstOrDefault(c => c.CarId.Equals(targetId, StringComparison.OrdinalIgnoreCase));

        if (attacker is null || target is null)
            throw new CardGameEngineException("Attacker or target car was not found.");

        attacker.Attack(this, target);
        CleanupDestroyedCars();
    }

    public void EndTurn(string playerId)
    {
        ValidatePlayerTurn(playerId);
        NextTurn();
    }

    public IReadOnlyList<StreetCarCard> GetHand(string playerId) =>
        GetPlayer(playerId).Hand.AllCards.OfType<StreetCarCard>().ToList();

    private void ValidatePlayerTurn(string playerId)
    {
        if (ActivePlayer != GetPlayer(playerId))
            throw new CardGameEngineException("It is not this player's turn.");
    }

    private static int Power(StreetCarCard card) => card.AttackValue + card.LifeValue;

    private void CleanupDestroyedCars()
    {
        foreach (var player in Players)
        foreach (var card in player.Board.AllCards.OfType<StreetCarCard>().Where(c => !c.IsAlive).ToList())
        {
            player.Board.Remove(card);
            player.Graveyard.Push(card);
        }
    }

    private static void BuildDeck(StreetWarsPlayer player)
    {
        var cars = new[]
        {
            ("Apex", 1, 3, 4), ("Comet", 1, 4, 3), ("Raptor", 1, 5, 4),
            ("Viper", 1, 4, 6), ("Shadow", 1, 7, 4), ("Titan", 1, 5, 8),
            ("Nitro", 1, 8, 5), ("Interceptor", 1, 6, 9)
        };

        foreach (var (model, mana, attack, life) in cars)
        {
            var id = $"{model.ToLowerInvariant()}-{Guid.CreateVersion7():N}";
            player.Deck.Push(new StreetCarCard(player, id, model, mana, attack, life));
        }

        player.Deck.Shuffle();
    }
}
