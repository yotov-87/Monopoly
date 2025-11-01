using Microsoft.EntityFrameworkCore;
using Monopoly.Core.DTOs;
using Monopoly.Core.Entities;
using Monopoly.Core.Interfaces;
using Monopoly.Data;

namespace Monopoly.Api.Services;

public class GameService : IGameService
{
    private readonly ApplicationDbContext _dbContext;

    public GameService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GameResponse?> CreateGameAsync(CreateGameRequest request, int userId)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return null;
        }

        if (request.PlayerCount < 2 || request.PlayerCount > 8)
        {
            return null;
        }

        // Check if user exists
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return null;
        }

        // Create game
        var game = new Game
        {
            Name = request.Name.Trim(),
            PlayerCount = request.PlayerCount,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            Status = "Waiting"
        };

        _dbContext.Games.Add(game);
        await _dbContext.SaveChangesAsync();

        // Load the user relation
        await _dbContext.Entry(game).Reference(g => g.CreatedBy).LoadAsync();

        return new GameResponse
        {
            Id = game.Id,
            Name = game.Name,
            PlayerCount = game.PlayerCount,
            CreatedBy = game.CreatedBy.Username,
            CreatedAt = game.CreatedAt,
            Status = game.Status
        };
    }

    public async Task<GameResponse?> GetGameByIdAsync(int gameId)
    {
        var game = await _dbContext.Games
            .Include(g => g.CreatedBy)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            return null;
        }

        return new GameResponse
        {
            Id = game.Id,
            Name = game.Name,
            PlayerCount = game.PlayerCount,
            CreatedBy = game.CreatedBy.Username,
            CreatedAt = game.CreatedAt,
            Status = game.Status
        };
    }

    public async Task<List<GameResponse>> GetAllGamesAsync()
    {
        var games = await _dbContext.Games
            .Include(g => g.CreatedBy)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return games.Select(g => new GameResponse
        {
            Id = g.Id,
            Name = g.Name,
            PlayerCount = g.PlayerCount,
            CreatedBy = g.CreatedBy.Username,
            CreatedAt = g.CreatedAt,
            Status = g.Status
        }).ToList();
    }

    public async Task<List<GameResponse>> GetGamesByUserAsync(int userId)
    {
        var games = await _dbContext.Games
            .Include(g => g.CreatedBy)
            .Where(g => g.CreatedByUserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return games.Select(g => new GameResponse
        {
            Id = g.Id,
            Name = g.Name,
            PlayerCount = g.PlayerCount,
            CreatedBy = g.CreatedBy.Username,
            CreatedAt = g.CreatedAt,
            Status = g.Status
        }).ToList();
    }
}
