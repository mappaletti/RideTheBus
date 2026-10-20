using Microsoft.AspNetCore.Mvc;
using RideTheBus.Api.Contracts.Games;
using RideTheBus.Application.Games;

namespace RideTheBus.Api.Controllers;

[ApiController]
[Route("games")]
public class GamesController : ControllerBase
{
    private readonly CreateGame _createGame;

    public GamesController(CreateGame createGame)
    {
        _createGame = createGame;
    }

    [HttpPost]
    public ActionResult<GameResponse> Create([FromBody] CreateGameRequest request)
    {
        var game = _createGame.Execute(request.Stake);
        var response = GameResponseMapper.From(game);
        return Ok(response);
    }
}