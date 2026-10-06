namespace RideTheBus.Game.Cards.Tests;

public class DeckTests
{
    [Fact]
    public void NewDeck_ShouldContain52Cards()
    {
        // Arrange
        var random = new Random(42);

        // Act 
        var deck = new Deck(random);

        // Assert
        Assert.Equal(52, deck.RemainingCount);
    }

    [Fact]
    public void Draw_ShouldDecreaseRemainingCountByOne()
    {
        // Arrange
        var random = new Random(42);
        var deck = new Deck(random);
        var initialCount = deck.RemainingCount;

        // Act 
        deck.Draw();

        // Assert
        Assert.Equal(initialCount - 1, deck.RemainingCount);
    }

    [Fact]
    public void DrawingAllCards_ShouldNotReturnDuplicates()
    {
        // Arrange
        var random = new Random(42);
        var deck = new Deck(random);
        var drawnCards = new HashSet<Card>();

        // Act & Assert
        while (deck.RemainingCount > 0)
        {
            var drawnCard = deck.Draw();
            var wasAdded = drawnCards.Add(drawnCard);

            Assert.True(wasAdded, $"Duplicate card was drawn: {drawnCard}");
        }
    }

    [Fact]
    public void DrawingFromEmptyDeck_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var random = new Random(42);
        var deck = new Deck(random);

        while (deck.RemainingCount > 0) deck.Draw();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => deck.Draw());
    }

    [Theory]
    [InlineData(Suit.Hearts, CardColor.Red)]
    [InlineData(Suit.Diamonds, CardColor.Red)]
    [InlineData(Suit.Clubs, CardColor.Black)]
    [InlineData(Suit.Spades, CardColor.Black)]
    public void Color_ShouldMatchSuit(Suit suit, CardColor color)
    {
        var card = new Card(suit, Rank.Two);

        Assert.Equal(color, card.Color);
    }
}