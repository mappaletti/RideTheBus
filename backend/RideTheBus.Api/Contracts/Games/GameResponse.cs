using RideTheBus.Game.Gameplay;

namespace RideTheBus.Api.Contracts.Games;

public sealed record GameResponse(
    Guid Id,
    decimal Stake,
    GameStatus Status,
    GameStage? Stage,
    IReadOnlyList<CardResponse> RevealedCards,
    decimal PotentialPayout,
    decimal? FinalPayout
);