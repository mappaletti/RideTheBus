using RideTheBus.Game.Cards;
using RideTheBus.Game.Gameplay;

namespace RideTheBus.Application.Games;

public sealed class GuessColor
{
    private readonly IGameSessionStore _store;

    public GuessColor(IGameSessionStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        
        _store = store;
    }

    public GameSession? Execute(Guid id, CardColor choice)
    {
        var game = _store.GetById(id);
        if (game == null) return null;
        
        game.GuessColor(choice);
        return game;
    }
}