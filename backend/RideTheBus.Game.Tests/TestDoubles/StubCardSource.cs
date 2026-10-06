using RideTheBus.Game.Cards;

namespace RideTheBus.Game.Tests.TestDoubles;

public sealed class StubCardSource : ICardSource
{
    private readonly Queue<Card> _cards;

    public StubCardSource(Card[] cards)
    {
        _cards = new Queue<Card>(cards);
    }

    public Card Draw()
    {
        return _cards.Dequeue();
    }
}