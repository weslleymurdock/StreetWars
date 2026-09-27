using StreetWars.Backend.Game;

namespace StreetWars.Backend.Services;

public interface IPVPService
{
    void PlayCar(StreetWarsGame game, string playerId, string cardId, int territory);
    void Attack(StreetWarsGame game, string playerId, string attackerId, string targetId);
    void EndTurn(StreetWarsGame game, string playerId);
}

public sealed class PVPService : IPVPService
{
    public void PlayCar(StreetWarsGame game, string playerId, string cardId, int territory) =>
        game.PlayCar(playerId, cardId, territory);

    public void Attack(StreetWarsGame game, string playerId, string attackerId, string targetId) =>
        game.Attack(playerId, attackerId, targetId);

    public void EndTurn(StreetWarsGame game, string playerId) =>
        game.EndTurn(playerId);
}
