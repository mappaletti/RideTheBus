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
    private readonly GuessColor _guessColor;

    public GamesController(CreateGame createGame, GetGame getGame, GuessColor guessColor)
    {
        _createGame = createGame;
        _getGame = getGame;
        _guessColor = guessColor;
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

    [HttpPost("{id:guid}/guess-color")]
    public ActionResult<GameResponse> GuessColor(Guid id, [FromBody] GuessColorRequest request)
    {
        try
        {
            var game = _guessColor.Execute(id, request.Choice);
            if (game == null) return NotFound();

            var response = GameResponseMapper.From(game);
            return Ok(response);
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest(new { message = "Unknown card color." });
        }
        catch (InvalidOperationException)
        {
            return Conflict(new {message="Color guess is not allowed in the current game state."});
        }
    }
}