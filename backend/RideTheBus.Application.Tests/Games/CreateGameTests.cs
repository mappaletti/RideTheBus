using RideTheBus.Application.Games;
using RideTheBus.Application.Tests.TestDoubles;
using RideTheBus.Game.Cards;
using RideTheBus.Game.Gameplay;

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

    [Fact]
    public void Execute_WhenGameExists_ShouldReturnStoredGame()
    {
        // arrange
        var store = new StubGameSessionStore();
        var game = new GameSession(100m, new Deck(new Random(42)));
        var useCase = new GetGame(store);
        store.GameToReturn = game;
        
        // act
        var foundGame = useCase.Execute(game.Id);
        
        // assert
        Assert.Same(game, foundGame);
        Assert.Equal(game.Id, store.RequestedId);
    }
}