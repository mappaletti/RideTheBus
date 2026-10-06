namespace RideTheBus.Game.Cards;

public sealed record Card(
    Suit Suit,
    Rank Rank
)
{
    public CardColor Color => Suit switch
    {
        Suit.Hearts or Suit.Diamonds => CardColor.Red,
        Suit.Spades or Suit.Clubs => CardColor.Black,

        _ => throw new ArgumentOutOfRangeException(nameof(Suit), Suit, "Unknown card suit")
    };
}