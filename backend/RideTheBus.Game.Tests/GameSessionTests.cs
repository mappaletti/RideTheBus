using RideTheBus.Game.Cards;
using RideTheBus.Game.Tests.TestDoubles;

namespace RideTheBus.Game.Gameplay.Tests;

public sealed class GameSessionTests
{
    [Fact]
    public void NewGame_ShouldStartInProgressAtColorStage()
    {
        // Arrange
        const decimal stake = 100m;
        var deck = new Deck(new Random(42));

        // Act
        var game = new GameSession(stake, deck);

        // Assert
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(GameStage.Color, game.Stage);
    }

    [Fact]
    public void NewGame_ShouldHaveNoRevealedCards()
    {
        // arrange
        const decimal stake = 100m;
        var deck = new Deck(new Random(42));

        // act
        var game = new GameSession(stake, deck);

        // assert
        Assert.Empty(game.RevealedCards);
    }

    [Fact]
    public void GuessColor_WhenCorrect_ShouldAdvanceToHigherOrLower()
    {
        // arrange
        var cardSource = new StubCardSource(new[]
        {
            new Card(Suit.Hearts, Rank.Two)
        });
        var game = new GameSession(100m, deck);

        // act
        // game.GuessColor(CardColor.Red);

        // assert
        Assert.Equal(GameStage.HigherOrLower, game.Stage);
    }
}