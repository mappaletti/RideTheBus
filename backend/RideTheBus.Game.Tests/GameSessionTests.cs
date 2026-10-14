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
        var cardSource = new StubCardSource(new[] { new Card(Suit.Hearts, Rank.Two) });
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
        var cardSource = new StubCardSource(new[] { new Card(Suit.Hearts, Rank.Two) });
        var game = new GameSession(100m, cardSource);

        // act assert
        Assert.Equal(0m, game.PotentialPayout);
    }

    [Fact]
    public void GuessColor_WhenIncorrect_ShouldHaveZeroPotentialPayout()
    {
        // arrange
        var cardSource = new StubCardSource(new[] { new Card(Suit.Hearts, Rank.Two) });
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
        var cardSource = new StubCardSource(new[] { new Card(Suit.Hearts, Rank.Two) });
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
        var cardSource = new StubCardSource(new[] { new Card(Suit.Hearts, Rank.Two) });
        var game = new GameSession(100m, cardSource);

        // act assert
        Assert.Throws<InvalidOperationException>(() => game.CashOut());

        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(GameStage.Color, game.Stage);
        Assert.Null(game.FinalPayout);
        Assert.Equal(0, cardSource.DrawCallCount);
    }

    [Fact]
    public void CashOut_AfterLoss_ShouldRejectWithoutChangingState()
    {
        // arrange
        var cardSource = new StubCardSource(new[] { new Card(Suit.Hearts, Rank.Two) });
        var game = new GameSession(100m, cardSource);

        game.GuessColor(CardColor.Black);
        var drawCallCount = cardSource.DrawCallCount;
        var finalPayout = game.FinalPayout;

        // act assert
        Assert.Throws<InvalidOperationException>(() => game.CashOut());
        Assert.Equal(GameStatus.Lost, game.Status);
        Assert.Null(game.Stage);
        Assert.Equal(finalPayout, game.FinalPayout);
        Assert.Equal(drawCallCount, cardSource.DrawCallCount);
    }

    [Fact]
    public void GuessColor_WhenIncorrect_ShouldFinalizeZeroPayout()
    {
        // arrange
        var cardSource = new StubCardSource(new[] { new Card(Suit.Hearts, Rank.Two) });
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Black);

        // act assert
        Assert.Equal(0m, game.FinalPayout);
    }

    [Fact]
    public void CashOut_WhenAlreadyCashedOut_ShouldRejectWithoutChangingState()
    {
        // arrange
        var cardSource = new StubCardSource(new[] { new Card(Suit.Hearts, Rank.Two) });
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);
        game.CashOut();
        var drawCallCount = cardSource.DrawCallCount;

        // act assert
        Assert.Throws<InvalidOperationException>(() => game.CashOut());
        Assert.Equal(GameStatus.CashedOut, game.Status);
        Assert.Null(game.Stage);
        Assert.Equal(200m, game.FinalPayout);
        Assert.Equal(0m, game.PotentialPayout);
        Assert.Equal(drawCallCount, cardSource.DrawCallCount);
    }

    [Fact]
    public void CashOut_AfterCorrectColor_ShouldNotDrawAnotherCard()
    {
        // arrange
        var cardSource = new StubCardSource(new[] { new Card(Suit.Hearts, Rank.Two), new Card(Suit.Spades, Rank.Two) });
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);
        var drawCallsBefore = cardSource.DrawCallCount;

        // act
        game.CashOut();

        // assert
        Assert.Equal(drawCallsBefore, cardSource.DrawCallCount);
    }

    [Fact]
    public void GuessHigherOrLower_WhenHigherIsCorrect_ShouldAdvanceToInsideOrOutside()
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            { new Card(Suit.Hearts, Rank.Five), new Card(Suit.Spades, Rank.Ten) });
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);

        // act
        game.GuessHigherOrLower(HigherOrLowerChoice.Higher);

        // assert 
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(GameStage.InsideOrOutside, game.Stage);
    }

    [Fact]
    public void GuessHigherOrLower_WhenLowerIsCorrect_ShouldAdvanceToInsideOrOutside()
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            { new Card(Suit.Hearts, Rank.Ten), new Card(Suit.Spades, Rank.Five) });
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);

        // act
        game.GuessHigherOrLower(HigherOrLowerChoice.Lower);

        // assert 
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(GameStage.InsideOrOutside, game.Stage);
    }

    [Fact]
    public void GuessHigherOrLower_WhenHigherIsIncorrect_ShouldLoseGame()
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            { new Card(Suit.Hearts, Rank.Ten), new Card(Suit.Spades, Rank.Five) });
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);

        // act
        game.GuessHigherOrLower(HigherOrLowerChoice.Higher);

        // assert 
        Assert.Null(game.Stage);
        Assert.Equal(GameStatus.Lost, game.Status);
        Assert.Equal(0m, game.FinalPayout);
    }

    [Fact]
    public void GuessHigherOrLower_WhenLowerIsIncorrect_ShouldLoseGame()
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            { new Card(Suit.Hearts, Rank.Five), new Card(Suit.Spades, Rank.Ten) });
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);

        // act
        game.GuessHigherOrLower(HigherOrLowerChoice.Lower);

        // assert 
        Assert.Null(game.Stage);
        Assert.Equal(GameStatus.Lost, game.Status);
        Assert.Equal(0m, game.FinalPayout);
    }

    [Theory]
    [InlineData(HigherOrLowerChoice.Higher)]
    [InlineData(HigherOrLowerChoice.Lower)]
    public void GuessHigherOrLower_WhenRanksAreEqual_ShouldLoseGame(HigherOrLowerChoice choice)
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            { new Card(Suit.Hearts, Rank.Five), new Card(Suit.Spades, Rank.Five) });
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);

        // act 
        game.GuessHigherOrLower(choice);

        // assert 
        Assert.Null(game.Stage);
        Assert.Equal(GameStatus.Lost, game.Status);
        Assert.Equal(0m, game.FinalPayout);
    }

    [Theory]
    [InlineData(Rank.Five, Rank.Ten, HigherOrLowerChoice.Higher)]
    [InlineData(Rank.Ten, Rank.Five, HigherOrLowerChoice.Lower)]
    public void GuessHigherOrLower_WhenCorrect_ShouldOfferTripleStakePayout(
        Rank firstRank,
        Rank secondRank,
        HigherOrLowerChoice choice)
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            { new Card(Suit.Hearts, firstRank), new Card(Suit.Spades, secondRank) });
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);

        // act
        game.GuessHigherOrLower(choice);

        // assert
        Assert.Equal(300m, game.PotentialPayout);
    }

    [Fact]
    public void CashOut_AfterCorrectHigherOrLower_ShouldFinalizeTripleStakePayout()
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            { new Card(Suit.Hearts, Rank.Five), new Card(Suit.Spades, Rank.Ten) });
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);

        // act
        game.GuessHigherOrLower(HigherOrLowerChoice.Higher);
        game.CashOut();

        // assert
        Assert.Equal(300m, game.FinalPayout);
        Assert.Null(game.Stage);
        Assert.Equal(GameStatus.CashedOut, game.Status);
        Assert.Equal(0m, game.PotentialPayout);
    }

    [Fact]
    public void GuessHigherOrLower_BeforeColorGuess_ShouldRejectWithoutChangingState()
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            { new Card(Suit.Hearts, Rank.Five), new Card(Suit.Spades, Rank.Ten) });
        var game = new GameSession(100m, cardSource);

        // act assert
        Assert.Throws<InvalidOperationException>(() => game.GuessHigherOrLower(HigherOrLowerChoice.Higher));
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(GameStage.Color, game.Stage);
        Assert.Empty(game.RevealedCards);
        Assert.Null(game.FinalPayout);
        Assert.Equal(0, cardSource.DrawCallCount);
    }

    [Fact]
    public void GuessHigherOrLower_WithInvalidChoice_ShouldRejectWithoutChangingState()
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            { new Card(Suit.Hearts, Rank.Five), new Card(Suit.Spades, Rank.Ten) });
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);
        var drawCallCount = cardSource.DrawCallCount;

        // act assert
        Assert.Throws<ArgumentOutOfRangeException>(() => game.GuessHigherOrLower((HigherOrLowerChoice)666));
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(GameStage.HigherOrLower, game.Stage);
        Assert.Single(game.RevealedCards);
        Assert.Null(game.FinalPayout);
        Assert.Equal(drawCallCount, cardSource.DrawCallCount);
        Assert.Equal(200m, game.PotentialPayout);
    }

    [Fact]
    public void GuessHigherOrLower_AfterCorrectGuess_ShouldRejectWithoutDrawingCard()
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            { new Card(Suit.Hearts, Rank.Five), new Card(Suit.Spades, Rank.Ten), new Card(Suit.Diamonds, Rank.Ace) });
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);
        game.GuessHigherOrLower(HigherOrLowerChoice.Higher);
        var drawCallCount = cardSource.DrawCallCount;

        // act assert
        Assert.Throws<InvalidOperationException>(() => game.GuessHigherOrLower(HigherOrLowerChoice.Higher));
        Assert.Equal(drawCallCount, cardSource.DrawCallCount);
        Assert.Equal(2, game.RevealedCards.Count);
        Assert.Equal(GameStage.InsideOrOutside, game.Stage);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GuessHigherOrLower_AfterGameEnds_ShouldRejectWithoutChangingState(
        bool cashOut)
    {
        // arrange
        var cardSource = new StubCardSource(new[]
        {
            new Card(Suit.Hearts, Rank.Five),
            new Card(Suit.Spades, Rank.Ten),
            new Card(Suit.Diamonds, Rank.Ace)
        });

        var game = new GameSession(100m, cardSource);

        // act
        if (cashOut)
        {
            game.GuessColor(CardColor.Red);
            game.CashOut();
        }
        else
        {
            game.GuessColor(CardColor.Black);
        }

        var status = game.Status;
        var finalPayout = game.FinalPayout;
        var drawCallCount = cardSource.DrawCallCount;

        // assert
        Assert.Throws<InvalidOperationException>(() => game.GuessHigherOrLower(HigherOrLowerChoice.Higher));

        Assert.Equal(status, game.Status);
        Assert.Equal(finalPayout, game.FinalPayout);
        Assert.Equal(drawCallCount, cardSource.DrawCallCount);
        Assert.Null(game.Stage);
        Assert.Single(game.RevealedCards);
    }
    
    [Theory]
    [InlineData(HigherOrLowerChoice.Higher)]
    [InlineData(HigherOrLowerChoice.Lower)]
    public void GuessHigherOrLower_ShouldRevealDrawnCard(
        HigherOrLowerChoice choice)
    {
        // arrange
        var expectedCard = new Card(Suit.Spades, Rank.Ten);

        var cardSource = new StubCardSource(new[]
        {
            new Card(Suit.Hearts, Rank.Five),
            expectedCard
        });

        var game = new GameSession(100m, cardSource);

        // act
        game.GuessColor(CardColor.Red);
        game.GuessHigherOrLower(choice);

        // assert
        Assert.Equal(2, game.RevealedCards.Count);
        Assert.Equal(expectedCard, game.RevealedCards[1]);
    }

    [Fact]
    public void GuessInsideOrOutside_WhenInsideIsCorrect_ShouldAdvanceToSuit()
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            {
                new Card(Suit.Hearts, Rank.Five),
                new Card(Suit.Spades, Rank.Ten),
                new Card(Suit.Diamonds, Rank.Seven),
            }
        );
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);
        game.GuessHigherOrLower(HigherOrLowerChoice.Higher);
        
        // act
        game.GuessInsideOrOutside(InsideOrOutsideChoice.Inside);
        
        // assert
        Assert.Equal(GameStage.Suit, game.Stage);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Theory]
    [InlineData(Rank.Three)]
    [InlineData(Rank.Queen)]
    public void GuessInsideOrOutside_WhenOutsideIsCorrect_ShouldAdvanceToSuit(
        Rank thirdRank)
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            {
                new Card(Suit.Hearts, Rank.Five),
                new Card(Suit.Spades, Rank.Ten),
                new Card(Suit.Diamonds, thirdRank),
            }
        );
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);
        game.GuessHigherOrLower(HigherOrLowerChoice.Higher);
        
        // act
        game.GuessInsideOrOutside(InsideOrOutsideChoice.Outside);
        
        // assert
        Assert.Equal(GameStage.Suit, game.Stage);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void GuessInsideOrOutside_WhenInsideChosenAndCardAboveRange_ShouldLoseGame()
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            {
                new Card(Suit.Hearts, Rank.Five),
                new Card(Suit.Spades, Rank.Ten),
                new Card(Suit.Diamonds, Rank.Ace),
            }
        );
        
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);
        game.GuessHigherOrLower(HigherOrLowerChoice.Higher);
        
        // act
        game.GuessInsideOrOutside(InsideOrOutsideChoice.Inside);
        
        // assert 
        Assert.Null(game.Stage);
        Assert.Equal(GameStatus.Lost, game.Status);
        Assert.Equal(0m, game.FinalPayout);
    }

    [Theory]
    [InlineData(Rank.Five, InsideOrOutsideChoice.Inside)]
    [InlineData(Rank.Five, InsideOrOutsideChoice.Outside)]
    [InlineData(Rank.Ten, InsideOrOutsideChoice.Inside)]
    [InlineData(Rank.Ten, InsideOrOutsideChoice.Outside)]
    public void GuessInsideOrOutside_WhenCardIsOnBoundary_ShouldLoseGame(
        Rank thirdRank,
        InsideOrOutsideChoice choice)
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            {
                new Card(Suit.Hearts, Rank.Five),
                new Card(Suit.Spades, Rank.Ten),
                new Card(Suit.Diamonds, thirdRank),
            }
        );
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);
        game.GuessHigherOrLower(HigherOrLowerChoice.Higher);
        
        // act
        game.GuessInsideOrOutside(choice);
        
        // assert 
        Assert.Null(game.Stage);
        Assert.Equal(GameStatus.Lost, game.Status);
        Assert.Equal(0m, game.FinalPayout);
    }

    [Theory]
    [InlineData(Rank.Seven, InsideOrOutsideChoice.Inside)]
    [InlineData(Rank.Three, InsideOrOutsideChoice.Outside)]
    [InlineData(Rank.Queen, InsideOrOutsideChoice.Outside)]
    public void GuessInsideOrOutside_WhenReferenceCardsAreDescending_ShouldAdvanceToSuit(
        Rank thirdRank,
        InsideOrOutsideChoice choice)
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            {
                new Card(Suit.Spades, Rank.Ten),
                new Card(Suit.Hearts, Rank.Five),
                new Card(Suit.Diamonds, thirdRank),
            }
        );
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Black);
        game.GuessHigherOrLower(HigherOrLowerChoice.Lower);
        
        // act 
        game.GuessInsideOrOutside(choice);
        
        // assert
        Assert.Equal(GameStage.Suit, game.Stage);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Theory]
    [InlineData(Rank.Seven, InsideOrOutsideChoice.Inside)]
    [InlineData(Rank.Queen, InsideOrOutsideChoice.Outside)]
    public void GuessInsideOrOutside_WhenCorrect_ShouldOfferQuadrupleStakePayout(
        Rank thirdRank,
        InsideOrOutsideChoice choice)
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            {
                new Card(Suit.Hearts, Rank.Five),
                new Card(Suit.Spades, Rank.Ten),
                new Card(Suit.Diamonds, thirdRank),
            }
        );
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);
        game.GuessHigherOrLower(HigherOrLowerChoice.Higher);
        
        // act
        game.GuessInsideOrOutside(choice);
        
        // assert
        Assert.Equal(400m, game.PotentialPayout);
    }

    [Fact]
    public void CashOut_AfterCorrectInsideOrOutside_ShouldFinalizeQuadrupleStakePayout()
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            {
                new Card(Suit.Hearts, Rank.Five),
                new Card(Suit.Spades, Rank.Ten),
                new Card(Suit.Diamonds, Rank.Seven),
            }
        );
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);
        game.GuessHigherOrLower(HigherOrLowerChoice.Higher);
        
        // act
        game.GuessInsideOrOutside(InsideOrOutsideChoice.Inside);
        game.CashOut();
        
        // assert
        Assert.Equal(400m, game.FinalPayout);
        Assert.Equal(GameStatus.CashedOut, game.Status);
        Assert.Null(game.Stage);
        Assert.Equal(0m, game.PotentialPayout);
    }

    [Fact]
    public void GuessSuit_WhenCorrect_ShouldWinWithTwentyfoldPayout()
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            {
                new Card(Suit.Hearts, Rank.Five),
                new Card(Suit.Spades, Rank.Ten),
                new Card(Suit.Diamonds, Rank.Seven),
                new Card(Suit.Clubs, Rank.Ace),
            }
        );
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);
        game.GuessHigherOrLower(HigherOrLowerChoice.Higher);
        game.GuessInsideOrOutside(InsideOrOutsideChoice.Inside);
        
        // act
        game.GuessSuit(Suit.Clubs);
        
        // assert
        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Null(game.Stage);
        Assert.Equal(2000m, game.FinalPayout);
        Assert.Equal(0m, game.PotentialPayout);
    }

    [Fact]
    public void GuessSuit_WhenIncorrect_ShouldLoseWithZeroPayout()
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            {
                new Card(Suit.Hearts, Rank.Five),
                new Card(Suit.Spades, Rank.Ten),
                new Card(Suit.Diamonds, Rank.Seven),
                new Card(Suit.Clubs, Rank.Ace),
            }
        );
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);
        game.GuessHigherOrLower(HigherOrLowerChoice.Higher);
        game.GuessInsideOrOutside(InsideOrOutsideChoice.Inside);
        
        // act
        game.GuessSuit(Suit.Hearts);
        
        // assert
        Assert.Equal(GameStatus.Lost, game.Status);
        Assert.Null(game.Stage);
        Assert.Equal(0m, game.FinalPayout);
        Assert.Equal(0m, game.PotentialPayout);
    }

    [Fact]
    public void GuessSuit_WithInvalidChoice_ShouldRejectWithoutChangingState()
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            {
                new Card(Suit.Hearts, Rank.Five),
                new Card(Suit.Spades, Rank.Ten),
                new Card(Suit.Diamonds, Rank.Seven),
                new Card(Suit.Clubs, Rank.Ace),
            }
        );
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);
        game.GuessHigherOrLower(HigherOrLowerChoice.Higher);
        game.GuessInsideOrOutside(InsideOrOutsideChoice.Inside);
        var drawCallCount = cardSource.DrawCallCount;
        
        // act assert
        Assert.Throws<ArgumentOutOfRangeException>(() => game.GuessSuit((Suit)666));
        Assert.Equal(drawCallCount, cardSource.DrawCallCount);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(GameStage.Suit, game.Stage);
        Assert.Equal(3, game.RevealedCards.Count);
        Assert.Equal(400m, game.PotentialPayout);
        Assert.Null(game.FinalPayout);
    }

    [Fact]
    public void GuessSuit_AfterWin_ShouldRejectWithoutChangingState()
    {
        // arrange
        var cardSource = new StubCardSource(new[]
            {
                new Card(Suit.Hearts, Rank.Five),
                new Card(Suit.Spades, Rank.Ten),
                new Card(Suit.Diamonds, Rank.Seven),
                new Card(Suit.Clubs, Rank.Ace),
            }
        );
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);
        game.GuessHigherOrLower(HigherOrLowerChoice.Higher);
        game.GuessInsideOrOutside(InsideOrOutsideChoice.Inside);
        game.GuessSuit(Suit.Clubs);
        var drawCallCount = cardSource.DrawCallCount;
        
        // act 
        Assert.Throws<InvalidOperationException>(() => game.GuessSuit(Suit.Clubs));
        Assert.Equal(drawCallCount, cardSource.DrawCallCount);
        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Null(game.Stage);
        Assert.Equal(2000m, game.FinalPayout);
        Assert.Equal(4, game.RevealedCards.Count);
    }

    [Theory]
    [InlineData(Suit.Spades)]
    [InlineData(Suit.Hearts)]
    public void GuessSuit_ShouldRevealDrawnCard(Suit choice)
    {
        // arrange
        var expectedCard = new Card(Suit.Spades, Rank.Ace);
        var cardSource = new StubCardSource(new[]
            {
                new Card(Suit.Hearts, Rank.Five),
                new Card(Suit.Spades, Rank.Ten),
                new Card(Suit.Diamonds, Rank.Seven),
                expectedCard,
            }
        );
        var game = new GameSession(100m, cardSource);
        game.GuessColor(CardColor.Red);
        game.GuessHigherOrLower(HigherOrLowerChoice.Higher);
        game.GuessInsideOrOutside(InsideOrOutsideChoice.Inside);
        
        // act
        game.GuessSuit(choice);
        
        // assert
        Assert.Equal(4, game.RevealedCards.Count);
        Assert.Equal(expectedCard, game.RevealedCards[3]);
    }

    [Fact]
    public void NewGame_WithNullCardSource_ShouldThrowArgumentNullException()
    {
        // arrange act assert
        Assert.Throws<ArgumentNullException>(
            () => new GameSession(100m, null!));
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(0)]
    [InlineData(0.5)]
    public void NewGame_WithStakeBelowMinimum_ShouldThrowArgumentOutOfRangeException(
        double stake)
    {
        // arrange
        var cardSource = new StubCardSource([]);
        
        // act assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameSession((decimal)stake, cardSource));
    }
     
    public static IEnumerable<object[]> StakesWithTooManyDecimalPlaces =>
        new[]
        {
            new object[] { 1.001m },
            new object[] { 100.999m }
        };
    
    [Theory]
    [MemberData(nameof(StakesWithTooManyDecimalPlaces))]
    public void NewGame_WithMoreThanTwoDecimalPlaces_ShouldThrowArgumentOutOfRangeException(
        decimal stake)
    {
        // arrange
        var cardSource = new StubCardSource([]);
        
        // act assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameSession(stake, cardSource));
    }
}