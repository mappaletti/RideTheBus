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

    public void GuessColor(CardColor choice)
    {
        if (Status != GameStatus.InProgress || Stage != GameStage.Color) throw new InvalidOperationException();

        if (!Enum.IsDefined(choice))
        {
            throw new ArgumentOutOfRangeException(nameof(choice), choice, "Unknown card color");
        }
        
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
        }
    }
}