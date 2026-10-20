using RideTheBus.Application.Games;
using RideTheBus.Game.Gameplay;

namespace RideTheBus.Application.Tests.TestDoubles;

public sealed class StubGameSessionStore : IGameSessionStore
{
    public GameSession? AddedGame { get; private set; }
    public GameSession? GameToReturn { get; set; }
    public Guid? RequestedId { get; private set; }
    
    public void Add(GameSession game)
    {
        AddedGame = game;
    }

    public GameSession? GetById(Guid id)
    {
        RequestedId = id;
        return  GameToReturn;
    }
}