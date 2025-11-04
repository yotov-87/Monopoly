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

    [HttpGet("playground-info/{gameId}")]
    [Authorize]
    public async Task<IActionResult> GetPlaygroundInfo(int gameId)
    {
        // Get user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var playgroundInfo = await _gameService.GetPlaygroundInfoAsync(gameId, userId);

        if (playgroundInfo == null)
        {
            return Forbid(); // User not authorized to access this game
        }

        return Ok(playgroundInfo);
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

    [HttpPost("{gameId}/add-player")]
    [Authorize]
    public async Task<IActionResult> AddPlayer(int gameId, [FromBody] AddPlayerRequest request)
    {
        // Get user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new { message = "Username is required." });
        }

        var response = await _gameService.AddPlayerToGameAsync(gameId, request.Username.Trim(), userId);

        if (response == null)
        {
            return BadRequest(new { message = "Failed to add player. Game may be full, user not found, user already in game, or you don't have permission." });
        }

        return Ok(response);
    }

    [HttpPost("{gameId}/end-turn")]
    [Authorize]
    public async Task<IActionResult> EndTurn(int gameId)
    {
        // Get user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var response = await _gameService.EndTurnAsync(gameId, userId);

        if (response == null)
        {
            return BadRequest(new { message = "Failed to end turn. You may not be in the game or it's not your turn." });
        }

        return Ok(response);
    }

    [HttpPost("{gameId}/move")]
    [Authorize]
    public async Task<IActionResult> MovePlayer(int gameId, [FromBody] MovePlayerRequest request)
    {
        // Get user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        if (request.Steps < 1 || request.Steps > 12)
        {
            return BadRequest(new { message = "Steps must be between 1 and 12." });
        }

        var response = await _gameService.MovePlayerAsync(gameId, userId, request.Steps);

        if (response == null)
        {
            return BadRequest(new { message = "Failed to move player. You may not be in the game or it's not your turn." });
        }

        return Ok(response);
    }

    [HttpPost("{gameId}/roll-dice")]
    [Authorize]
    public async Task<IActionResult> RollDice(int gameId, [FromBody] RollDiceRequest request)
    {
        // Get user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        if (request.Dice1 < 1 || request.Dice1 > 6 || request.Dice2 < 1 || request.Dice2 > 6)
        {
            return BadRequest(new { message = "Dice values must be between 1 and 6." });
        }

        var success = await _gameService.BroadcastDiceRollAsync(gameId, userId, request.Dice1, request.Dice2);

        if (!success)
        {
            return BadRequest(new { message = "Failed to broadcast dice roll. You may not be in the game or it's not your turn." });
        }

        return Ok(new { message = "Dice roll broadcasted successfully." });
    }

    [HttpPost("{gameId}/ready")]
    [Authorize]
    public async Task<IActionResult> SetPlayerReady(int gameId, [FromBody] SetPlayerReadyRequest request)
    {
        // Get user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var success = await _gameService.SetPlayerReadyAsync(gameId, userId, request.IsReady);

        if (!success)
        {
            return NotFound(new { message = "Game not found or player not in game." });
        }

        return Ok(new { message = "Ready status updated successfully." });
    }

    [HttpPost("{gameId}/purchase-property/{cellId}")]
    [Authorize]
    public async Task<IActionResult> PurchaseProperty(int gameId, int cellId)
    {
        // Get user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        try
        {
            var playgroundInfo = await _gameService.PurchasePropertyAsync(gameId, userId, cellId);

            if (playgroundInfo == null)
            {
                return NotFound(new { message = "Game or cell not found." });
            }

            return Ok(playgroundInfo);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{gameId}/pay-rent/{cellId}")]
    public async Task<IActionResult> PayRent(int gameId, int cellId)
    {
        // Get user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        try
        {
            var playgroundInfo = await _gameService.PayRentAsync(gameId, userId, cellId);

            if (playgroundInfo == null)
            {
                return NotFound(new { message = "Game or cell not found." });
            }

            return Ok(playgroundInfo);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{gameId}/propose-trade/{cellId}")]
    public async Task<IActionResult> ProposeTrade(int gameId, int cellId, [FromBody] ProposeTradeRequest request)
    {
        // Get user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        try
        {
            var tradeId = await _gameService.ProposeTradeAsync(gameId, userId, cellId, request.OfferedPrice);

            if (tradeId == null)
            {
                return NotFound(new { message = "Failed to create trade offer." });
            }

            return Ok(new { tradeId, message = "Trade offer sent successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{gameId}/respond-trade/{tradeId}")]
    public async Task<IActionResult> RespondToTrade(int gameId, int tradeId, [FromBody] RespondTradeRequest request)
    {
        // Get user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        try
        {
            var playgroundInfo = await _gameService.RespondToTradeAsync(gameId, userId, tradeId, request.Accept);

            if (playgroundInfo == null)
            {
                return NotFound(new { message = "Trade or game not found." });
            }

            return Ok(playgroundInfo);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
