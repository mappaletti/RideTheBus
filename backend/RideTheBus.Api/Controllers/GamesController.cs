using Microsoft.AspNetCore.Mvc;
using RideTheBus.Api.Contracts.Games;
using RideTheBus.Application.Games;

namespace RideTheBus.Api.Controllers;

[ApiController]
[Route("games")]
public class GamesController : ControllerBase
{
    private readonly CreateGame _createGame;
    private readonly GetGame _getGame;

    public GamesController(CreateGame createGame, GetGame getGame)
    {
        _createGame = createGame;
        _getGame = getGame;
    }

    [HttpPost]
    public ActionResult<GameResponse> Create([FromBody] CreateGameRequest request)
    {
        var game = _createGame.Execute(request.Stake);
        var response = GameResponseMapper.From(game);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public ActionResult<GameResponse> Get(Guid id)
    {
        var game = _getGame.Execute(id);
        if (game == null) return NotFound();
        
        var response = GameResponseMapper.From(game);
        return Ok(response);
    }
}