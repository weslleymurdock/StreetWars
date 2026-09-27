using CardGameEngine;

namespace StreetWars.Backend.Game;

public sealed class StreetWarsGame : CardGameEngine.Game
{
    public const int TerritoryCount = 5;
    public const int TerritoryVictoryCount = 3;
    public const int InitialLife = 20;
    public const int InitialHandSize = 4;

    private StreetWarsGame(List<IPlayer> players) : base(players) { }

    public static StreetWarsGame Create(bool withAi)
    {
        var players = new List<IPlayer>
        {
            new StreetWarsPlayer("A", new Deck())
        };

        var game = new StreetWarsGame(players)
        {
            ActivePlayer = players[0]
        };

        InitializePlayer((StreetWarsPlayer)players[0]);

        if (withAi)
            game.AddPlayer("B");

        return game;
    }

    public StreetWarsPlayer GetPlayer(string playerId) =>
        Players.OfType<StreetWarsPlayer>()
            .FirstOrDefault(p => p.PlayerId.Equals(playerId, StringComparison.OrdinalIgnoreCase))
        ?? throw new CardGameEngineException($"Player '{playerId}' was not found.");

    public string AddPlayer()
    {
        var playerId = ((char)('A' + Players.Count)).ToString();
        AddPlayer(playerId);
        return playerId;
    }

    public void AddPlayer(string playerId)
    {
        if (Players.Count >= 4)
            throw new CardGameEngineException("The game supports at most four players.");

        if (Players.OfType<StreetWarsPlayer>().Any(p => p.PlayerId.Equals(playerId, StringComparison.OrdinalIgnoreCase)))
            throw new CardGameEngineException($"Player '{playerId}' already exists.");

        var player = new StreetWarsPlayer(playerId, new Deck());
        Players.Add(player);
        InitializePlayer(player);
    }

    public bool IsGameOver(out string? winner)
    {
        winner = null;

        if (Players.Count < 2)
            return false;

        foreach (var player in Players.OfType<StreetWarsPlayer>())
        {
            if (GetControlledTerritories(player).Count >= TerritoryVictoryCount)
            {
                winner = player.PlayerId;
                return true;
            }
        }

        var alive = Players.OfType<StreetWarsPlayer>().Where(p => p.IsAlive).ToList();
        if (alive.Count == 1)
        {
            winner = alive[0].PlayerId;
            return true;
        }

        return false;
    }

    public IReadOnlyList<int> GetControlledTerritories(IPlayer player)
    {
        var result = new List<int>();

        for (var territory = 0; territory < TerritoryCount; territory++)
        {
            var occupants = Players
                .Select(p => (Player: p, Card: GetCardAt(p, territory)))
                .Where(x => x.Card is not null)
                .Select(x => (x.Player, Card: x.Card!))
                .OrderByDescending(x => Power(x.Card))
                .ToList();

            if (occupants.Count > 0 &&
                occupants[0].Player == player &&
                (occupants.Count == 1 || Power(occupants[0].Card) > Power(occupants[1].Card)))
            {
                result.Add(territory);
            }
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
        var attacker = player.Board.AllCards.OfType<StreetCarCard>()
            .FirstOrDefault(c => c.CarId.Equals(attackerId, StringComparison.OrdinalIgnoreCase));

        var target = Players
            .Where(p => p != player)
            .SelectMany(p => p.Board.AllCards.OfType<StreetCarCard>())
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

    private static void InitializePlayer(StreetWarsPlayer player)
    {
        BuildDeck(player);

        player.ManaValue = 0;
        player.ManaBaseValue = 0;
        player.LifeValue = InitialLife;
        player.LifeBaseValue = InitialLife;

        // Draw directly during room setup so late joiners receive a normal opening hand.
        for (var i = 0; i < InitialHandSize; i++)
        {
            if (!player.Deck.IsEmpty)
            {
                var card = player.Deck.Pop();
                player.Hand.Push(card);
            }
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
