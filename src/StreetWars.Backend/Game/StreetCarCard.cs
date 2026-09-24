using CardGameEngine;

namespace StreetWars.Backend.Game;

public sealed class StreetWarsPlayer : Player
{
    public StreetWarsPlayer(IDeck deck) : base(deck)
    {
    }
}

public sealed class StreetCarComponent : MonsterCardComponent
{
    public string Part { get; }

    public StreetCarComponent(int mana, int attack, int life, string part)
        : base(mana, attack, life)
    {
        Part = part;
    }
}

public sealed class StreetCarCard : MonsterCard
{
    public string CarId => Name;
    public string Model { get; }

    public StreetCarCard(IPlayer owner, string id, string model, int mana, int attack, int life)
        : base([new StreetCarComponent(mana, attack, life, "Engine")], owner, id)
    {
        Model = model;
    }
}
