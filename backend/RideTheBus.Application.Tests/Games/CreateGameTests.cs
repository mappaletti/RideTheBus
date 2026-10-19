using RideTheBus.Application.Games;
using RideTheBus.Application.Tests.TestDoubles;

namespace RideTheBus.Application.Tests.Games;

public class CreateGameTests
{
    [Fact]
    public void Execute_ShouldCreateAndStoreGameWithRequestedStake()
    {
        // arrange
        var store = new StubGameSessionStore();
        var useCase = new CreateGame(store);

        // act 
        var game = useCase.Execute(100m);
        
        // assert
        Assert.Equal(100m, game.Stake);
        Assert.Same(game, store.AddedGame);
    }
    
    [Fact]
    public void Constructor_WithNullStore_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CreateGame(null!));
    }
}