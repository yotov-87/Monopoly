using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monopoly.Core.DTOs;
using Monopoly.Core.Interfaces;

namespace Monopoly.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
    {
        // Get user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var response = await _gameService.CreateGameAsync(request, userId);
        
        if (response == null)
        {
            return BadRequest(new { message = "Failed to create game. Invalid data provided." });
        }

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGame(int id)
    {
        var response = await _gameService.GetGameByIdAsync(id);
        
        if (response == null)
        {
            return NotFound(new { message = "Game not found." });
        }

        return Ok(response);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllGames()
    {
        var games = await _gameService.GetAllGamesAsync();
        return Ok(games);
    }

    [HttpGet("my-games")]
    [Authorize]
    public async Task<IActionResult> GetMyGames()
    {
        // Get user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var games = await _gameService.GetGamesByUserAsync(userId);
        return Ok(games);
    }
}
