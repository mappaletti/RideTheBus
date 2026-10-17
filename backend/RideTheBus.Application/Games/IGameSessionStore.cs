using RideTheBus.Game.Gameplay;

namespace RideTheBus.Application.Games;

public interface IGameSessionStore
{
    void Add(GameSession game);
    GameSession? GetById(Guid id);
}