using CardGameEngine;

namespace StreetWars.Backend.Game;

public sealed class StreetWarsPlayer(string playerId, IDeck deck) : Player(deck)
{
    public string PlayerId { get; } = playerId;
}

public sealed class StreetCarComponent(int mana, int attack, int life, string part) : MonsterCardComponent(mana, attack, life)
{
    public string Part { get; } = part;
}

public sealed class StreetCarCard(IPlayer owner, string id, string model, int mana, int attack, int life) : MonsterCard([new StreetCarComponent(mana, attack, life, "Engine")], owner)
{
    public string CarId { get; } = id;
    public string Model { get; } = model;
}
