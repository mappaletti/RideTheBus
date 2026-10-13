using RideTheBus.Game.Cards;

namespace RideTheBus.Game.Gameplay;

public sealed class GameSession
{
    private readonly ICardSource _cardSource;
    private readonly List<Card> _revealedCards = new();

    public GameSession(decimal stake, ICardSource cardSource)
    {
        Stake = stake;
        Status = GameStatus.InProgress;
        Stage = GameStage.Color;
        _cardSource = cardSource;
        RevealedCards = _revealedCards.AsReadOnly();
    }

    public IReadOnlyList<Card> RevealedCards { get; }
    public decimal Stake { get; }
    public GameStatus Status { get; private set; }
    public GameStage? Stage { get; private set; }

    public decimal PotentialPayout =>
        Status != GameStatus.InProgress
            ? 0m
            : Stage switch
            {
                GameStage.HigherOrLower => Stake * 2m,
                GameStage.InsideOrOutside => Stake * 3m,
                GameStage.Suit => Stake * 4m,
                _ => 0m
            };

    public decimal? FinalPayout { get; private set; }

    public void CashOut()
    {
        if (Status != GameStatus.InProgress || PotentialPayout <= 0m) throw new InvalidOperationException();

        FinalPayout = PotentialPayout;
        Status = GameStatus.CashedOut;
        Stage = null;
    }

    public void GuessColor(CardColor choice)
    {
        if (Status != GameStatus.InProgress || Stage != GameStage.Color) throw new InvalidOperationException();

        if (!Enum.IsDefined(choice))
            throw new ArgumentOutOfRangeException(nameof(choice), choice, "Unknown card color");

        var card = _cardSource.Draw();
        _revealedCards.Add(card);

        if (choice == card.Color)
        {
            Stage = GameStage.HigherOrLower;
        }
        else
        {
            Stage = null;
            Status = GameStatus.Lost;
            FinalPayout = 0m;
        }
    }

    public void GuessHigherOrLower(HigherOrLowerChoice choice)
    {
        if (Status != GameStatus.InProgress || Stage != GameStage.HigherOrLower) throw new InvalidOperationException();

        if (!Enum.IsDefined(choice))
            throw new ArgumentOutOfRangeException(nameof(choice), choice, "Unknown higher or lower choice");

        var referenceCard = _revealedCards[0];
        var card = _cardSource.Draw();
        _revealedCards.Add(card);

        if (card.Rank > referenceCard.Rank && choice == HigherOrLowerChoice.Higher)
        {
            Stage = GameStage.InsideOrOutside;
        }
        else if (card.Rank < referenceCard.Rank && choice == HigherOrLowerChoice.Lower)
        {
            Stage = GameStage.InsideOrOutside;
        }
        else
        {
            Stage = null;
            Status = GameStatus.Lost;
            FinalPayout = 0m;
        }
    }

    public void GuessInsideOrOutside(InsideOrOutsideChoice choice)
    {
        if (Status != GameStatus.InProgress || Stage != GameStage.InsideOrOutside) throw new InvalidOperationException();
        
        if (!Enum.IsDefined(choice))
                     throw new ArgumentOutOfRangeException(nameof(choice), choice, "Unknown inside or outside choice");
        
        var lowerRank = _revealedCards[0].Rank < _revealedCards[1].Rank
            ? _revealedCards[0].Rank
            : _revealedCards[1].Rank;
        
        var higherRank = _revealedCards[0].Rank > _revealedCards[1].Rank
            ? _revealedCards[0].Rank
            : _revealedCards[1].Rank;
        
        var card =  _cardSource.Draw();
        _revealedCards.Add(card);

        if (choice == InsideOrOutsideChoice.Inside && card.Rank > lowerRank && card.Rank < higherRank)
        {
            Stage = GameStage.Suit;
        }
        else if (choice == InsideOrOutsideChoice.Outside && (card.Rank < lowerRank || card.Rank > higherRank))
        {
            Stage = GameStage.Suit;
        }
        else
        {
            Stage = null;
            Status = GameStatus.Lost;
            FinalPayout = 0m;
        }
    }

    public void GuessSuit(Suit choice)
    {
        if (Status != GameStatus.InProgress || Stage != GameStage.Suit) throw new InvalidOperationException();
        
        if (!Enum.IsDefined(choice))
            throw new ArgumentOutOfRangeException(nameof(choice), choice, "Unknown suit choice");
        
        var card =  _cardSource.Draw();
        _revealedCards.Add(card);

        if (choice == card.Suit)
        {
            FinalPayout = Stake * 20m;
            Status = GameStatus.Won;
            Stage = null;
        }
        else
        {
            Stage = null;
            Status = GameStatus.Lost;
            FinalPayout = 0m;
        }
    }
}