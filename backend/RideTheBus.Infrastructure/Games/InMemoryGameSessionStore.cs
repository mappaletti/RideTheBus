using System.Collections.Concurrent;
using RideTheBus.Application.Games;
using RideTheBus.Game.Gameplay;

namespace RideTheBus.Infrastructure.Games;

public sealed class InMemoryGameSessionStore : IGameSessionStore
{
    private readonly ConcurrentDictionary<Guid, GameSession> _games = new();
    
    public void Add(GameSession game)
    {
        ArgumentNullException.ThrowIfNull(game);
        
        var wasAdded = _games.TryAdd(game.Id, game);
        
        if (!wasAdded) throw new InvalidOperationException();
    }

    public GameSession? GetById(Guid id)
    {
        var wasFound = _games.TryGetValue(id, out var game);
        
        return wasFound ? game : null;
    }
}