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
        if (!IsAiTurn(game))
            return;

        var aiPlayer = game.GetPlayer("B");

        var card = game.GetHand(aiPlayer.PlayerId)
            .Where(c => c.ManaValue <= aiPlayer.ManaValue)
            .OrderByDescending(c => c.AttackValue + c.LifeValue)
            .FirstOrDefault();

        var territory = FindBestTerritory(game, aiPlayer);
        if (card is not null && territory >= 0)
            game.PlayCar(aiPlayer.PlayerId, card.CarId, territory);

        var attack = FindAttack(game, aiPlayer);
        if (attack is not null)
            game.Attack(aiPlayer.PlayerId, attack.Value.attacker, attack.Value.target);

        if (!game.IsGameOver(out _))
            game.EndTurn(aiPlayer.PlayerId);
    }

    private static bool IsAiTurn(StreetWarsGame game) =>
        game.Players.Count >= 2 &&
        game.ActivePlayer is StreetWarsPlayer player &&
        player.PlayerId.Equals("B", StringComparison.OrdinalIgnoreCase);

    private static int FindBestTerritory(StreetWarsGame game, StreetWarsPlayer aiPlayer) =>
        Enumerable.Range(0, StreetWarsGame.TerritoryCount)
            .Where(i => game.GetCardAt(aiPlayer, i) is null)
            .OrderBy(i => game.Players
                .OfType<StreetWarsPlayer>()
                .Any(player => player.PlayerId != aiPlayer.PlayerId && game.GetCardAt(player, i) is not null) ? 1 : 0)
            .DefaultIfEmpty(-1)
            .First();

    private static (string attacker, string target)? FindAttack(
        StreetWarsGame game,
        StreetWarsPlayer aiPlayer)
    {
        var target = game.Players
            .OfType<StreetWarsPlayer>()
            .Where(player => player.PlayerId != aiPlayer.PlayerId)
            .SelectMany(player => player.Board.AllCards.OfType<StreetCarCard>())
            .FirstOrDefault();

        if (target is null)
            return null;

        var attacker = aiPlayer.Board.AllCards.OfType<StreetCarCard>()
            .FirstOrDefault(c => c.IsReadyToAttack);

        return attacker is null ? null : (attacker.CarId, target.CarId);
    }
}
