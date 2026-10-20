using RideTheBus.Game.Cards;
using RideTheBus.Game.Gameplay;

namespace RideTheBus.Application.Games;

public sealed class CreateGame
{
    private readonly IGameSessionStore _store;

    public CreateGame(IGameSessionStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        _store = store; 
    }

    public GameSession Execute(decimal stake)
    {
        var deck = new Deck(Random.Shared);
        var game = new GameSession(stake, deck);
        _store.Add(game);
        return game;
    }
}