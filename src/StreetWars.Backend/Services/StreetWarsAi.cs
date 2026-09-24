using StreetWars.Backend.Game;

namespace StreetWars.Backend.Services;

public interface IStreetWarsAi
{
    void PlayTurn(StreetWarsGame game);
}

public sealed class StreetWarsAi : IStreetWarsAi
{
    public void PlayTurn(StreetWarsGame game)
    {
        if (game.ActivePlayer != game.StreetPlayerB || game.IsGameOver(out _))
            return;

        var card = game.GetHand("B")
            .Where(c => c.ManaValue <= game.StreetPlayerB.ManaValue)
            .OrderByDescending(c => c.AttackValue + c.LifeValue)
            .FirstOrDefault();

        var territory = FindBestTerritory(game);
        if (card is not null && territory >= 0)
            game.PlayCar("B", card.CarId, territory);

        var attack = FindAttack(game);
        if (attack is not null)
            game.Attack("B", attack.Value.attacker, attack.Value.target);

        if (!game.IsGameOver(out _))
            game.EndTurn("B");
    }

    private static int FindBestTerritory(StreetWarsGame game) =>
        Enumerable.Range(0, StreetWarsGame.TerritoryCount)
            .Where(i => game.GetCardAt(game.StreetPlayerB, i) is null)
            .OrderBy(i => game.GetCardAt(game.StreetPlayerA, i) is null ? 0 : 1)
            .DefaultIfEmpty(-1)
            .First();

    private static (string attacker, string target)? FindAttack(StreetWarsGame game)
    {
        var target = game.StreetPlayerA.Board.AllCards.OfType<StreetCarCard>().FirstOrDefault();
        if (target is null) return null;

        var attacker = game.StreetPlayerB.Board.AllCards.OfType<StreetCarCard>()
            .FirstOrDefault(c => c.IsReadyToAttack);

        return attacker is null ? null : (attacker.CarId, target.CarId);
    }
}
