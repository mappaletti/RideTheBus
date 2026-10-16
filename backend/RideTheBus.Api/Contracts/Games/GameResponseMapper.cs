using RideTheBus.Game.Gameplay;

namespace RideTheBus.Api.Contracts.Games;

public static class GameResponseMapper
{
    public static GameResponse From(GameSession game)
    {
        IReadOnlyList<CardResponse> revealedCards = game.RevealedCards
            .Select(card => new CardResponse(card.Suit, card.Rank, card.Color))
            .ToArray();

        return new GameResponse(
            game.Id,
            game.Stake,
            game.Status,
            game.Stage,
            revealedCards,
            game.PotentialPayout,
            game.FinalPayout);
    }
}