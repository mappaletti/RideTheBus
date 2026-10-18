using Microsoft.AspNetCore.Mvc;
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
}