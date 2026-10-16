using RideTheBus.Game.Cards;

namespace RideTheBus.Api.Contracts.Games;

public sealed record CardResponse (
    Suit Suit,
    Rank Rank,
    CardColor Color
    );