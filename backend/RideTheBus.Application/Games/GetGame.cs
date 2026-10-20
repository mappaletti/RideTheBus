using RideTheBus.Game.Gameplay;

namespace RideTheBus.Application.Games;

public sealed class GetGame
{
    private readonly IGameSessionStore _store;
    
    public GetGame(IGameSessionStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        
        _store = store;
    }

    public GameSession? Execute(Guid id)
    {
        var game = _store.GetById(id);
        return game;
    }
}