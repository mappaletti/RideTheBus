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
        var game = new GameSession(100m, cardSource);

        // act
        game.GuessColor(CardColor.Red);

        // assert
        Assert.Equal(GameStage.HigherOrLower, game.Stage);
    }

    [Fact]
    public void GuessColor_WhenIncorrect_ShouldLoseGame()
    {
        // arrange
        var cardSource = new StubCardSource(new[]
        {
            new Card(Suit.Hearts, Rank.Two)
        });
        var game = new GameSession(100m, cardSource);

        // act
        game.GuessColor(CardColor.Black);

        // assert
        Assert.Null(game.Stage);
        Assert.Equal(GameStatus.Lost, game.Status);
    }

    [Fact]
    public void GuessColor_WhenIncorrect_ShouldRevealDrawnCard()
    {
        // arrange
        var expectedCard = new Card(Suit.Hearts, Rank.Two);
        var cardSource = new StubCardSource(new[] { expectedCard });
        var game = new GameSession(100m, cardSource);

        // act
        game.GuessColor(CardColor.Black);

        // assert
        var revealedCard = Assert.Single(game.RevealedCards);
        Assert.Equal(expectedCard, revealedCard);
    }

    [Fact]
    public void GuessColor_AfterLoss_ShouldRejectWithoutDrawingCard()
    {
        // arrange
        var cardSource = new StubCardSource(new[] { new Card(Suit.Hearts, Rank.Two), new Card(Suit.Spades, Rank.Two) });
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Black);
        var drawCallsBefore = cardSource.DrawCallCount;
        
        // act & assert
        Assert.Throws<InvalidOperationException>(() => game.GuessColor(CardColor.Red));
        Assert.Equal(drawCallsBefore, cardSource.DrawCallCount);
    }

    [Fact]
    public void GuessColor_WithInvalidColor_ShouldRejectWithoutDrawingCard()
    {
        // arrange
        var cardSource = new StubCardSource(new[] { new Card(Suit.Hearts, Rank.Two), new Card(Suit.Spades, Rank.Two) });
        var game = new GameSession(100m, cardSource);
        var drawCallsBefore = cardSource.DrawCallCount;
        
        // act assert
        Assert.Throws<ArgumentOutOfRangeException>(() => game.GuessColor((CardColor)666));
        Assert.Equal(drawCallsBefore, cardSource.DrawCallCount);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(GameStage.Color, game.Stage);
        Assert.Empty(game.RevealedCards);
    }

    [Fact]
    public void GuessColor_WhenCorrect_ShouldOfferDoubleStakePayout()
    {
        // arrange
        var cardSource = new StubCardSource(new[] { new Card(Suit.Hearts, Rank.Two)});
        var game = new GameSession(100m, cardSource);
        
        // act
        game.GuessColor(CardColor.Red);
        
        // assert
        Assert.Equal(200m, game.PotentialPayout);
    }

    [Fact]
    public void NewGame_ShouldHaveZeroPotentialPayout()
    {
        // arrange
        var cardSource = new StubCardSource(new[] { new Card(Suit.Hearts, Rank.Two)});
        var game = new GameSession(100m, cardSource);
        
        // act assert
        Assert.Equal(0m, game.PotentialPayout);
    }

    [Fact]
    public void GuessColor_WhenIncorrect_ShouldHaveZeroPotentialPayout()
    {
        // arrange
        var cardSource = new StubCardSource(new[] { new Card(Suit.Hearts, Rank.Two)});
        var game = new GameSession(100m, cardSource);
        
        // act
        game.GuessColor(CardColor.Black);
        
        // assert
        Assert.Equal(0m, game.PotentialPayout);
    }

    [Fact]
    public void CashOut_AfterCorrectColor_ShouldFinalizeDoubleStakePayout()
    {
        // arrange
        var cardSource = new StubCardSource(new[] { new Card(Suit.Hearts, Rank.Two)});
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);
        
        // act 
        game.CashOut();
        
        // assert
        Assert.Equal(200m, game.FinalPayout);
        Assert.Equal(GameStatus.CashedOut, game.Status);
        Assert.Null(game.Stage);
    }

    [Fact]
    public void CashOut_BeforeFirstCorrectGuess_ShouldRejectWithoutChangingState()
    {
        // arrange
        var cardSource = new StubCardSource(new[] { new Card(Suit.Hearts, Rank.Two)});
        var game = new GameSession(100m, cardSource);
        
        // act assert
        Assert.Throws<InvalidOperationException>(() => game.CashOut());
        
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(GameStage.Color, game.Stage);
        Assert.Null(game.FinalPayout);
        Assert.Equal(0, cardSource.DrawCallCount);
    }
}