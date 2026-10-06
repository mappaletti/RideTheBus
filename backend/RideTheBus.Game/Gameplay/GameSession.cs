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
}