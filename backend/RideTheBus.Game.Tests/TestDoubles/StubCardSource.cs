using RideTheBus.Game.Cards;
using RideTheBus.Game.Gameplay;

namespace RideTheBus.Game.Tests.TestDoubles;

public sealed class StubCardSource : ICardSource
{
    private readonly Queue<Card> _cards;
    public int DrawCallCount { get; private set; }

    public StubCardSource(Card[] cards)
    {
        _cards = new Queue<Card>(cards);
    }

    public Card Draw()
    {
        DrawCallCount++;
        return _cards.Dequeue();
    }
}