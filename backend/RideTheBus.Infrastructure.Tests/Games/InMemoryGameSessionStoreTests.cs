using RideTheBus.Game.Cards;
using RideTheBus.Game.Gameplay;
using RideTheBus.Infrastructure.Games;

namespace RideTheBus.Infrastructure.Tests.Games;

public class InMemoryGameSessionStoreTests
{
    [Fact]
    public void GetById_WhenGameWasAdded_ShouldReturnSameGame()
    {
        // arrange
        var store = new InMemoryGameSessionStore();
        var deck = new Deck(new Random(42));
        var game = new GameSession(100m, deck);
        store.Add(game);
        
        // act
        var result = store.GetById(game.Id);
        
        // assert
        Assert.Same(game, result);
    }
    
    [Fact]
    public void GetById_WhenGameDoesNotExist_ShouldReturnNull()
    {
        // arrange
        var store = new InMemoryGameSessionStore();
        
        // act
        var result = store.GetById(Guid.NewGuid());
        
        // assert
        Assert.Null(result);
    }

    [Fact]
    public void Add_WhenGameAlreadyExists_ShouldThrowWithoutRemovingStoredGame()
    {
        // arrange
        var store = new InMemoryGameSessionStore();
        var deck = new Deck(new Random(42));
        var game = new GameSession(100m, deck);
        store.Add(game);
        
        // act assert
        Assert.Throws<InvalidOperationException>(() => store.Add(game));
        Assert.Same(game, store.GetById(game.Id));
    }

    [Fact]
    public void Add_WithNullGame_ShouldThrowArgumentNullException()
    {
        // arrange
        var store = new InMemoryGameSessionStore();
        
        // act assert
        Assert.Throws<ArgumentNullException>(() => store.Add(null!));
    }
}