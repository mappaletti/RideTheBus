namespace RideTheBus.Game.Cards;

public sealed class Deck : ICardSource
{
    private readonly List<Card> _cards = new();

    public Deck(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);

        foreach (var suit in Enum.GetValues<Suit>())
        foreach (var rank in Enum.GetValues<Rank>())
            _cards.Add(new Card(suit, rank));

        Shuffle(random);
    }

    public int RemainingCount => _cards.Count;

    public Card Draw()
    {
        if (_cards.Count == 0) throw new InvalidOperationException("Cannot draw from an empty deck.");

        var card = _cards[^1];
        _cards.RemoveAt(_cards.Count - 1);

        return card;
    }

    private void Shuffle(Random random)
    {
        for (var i = _cards.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);

            (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
        }
    }
}