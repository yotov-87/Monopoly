using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Monopoly.Core.DTOs;
using Monopoly.Core.Entities;
using Monopoly.Core.Interfaces;
using Monopoly.Data;
using Monopoly.Hubs;

namespace Monopoly.Api.Services;

public class GameService : IGameService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IBoardGenerator _boardGenerator;
    private readonly IHubContext<GameHub> _hubContext;

    public GameService(
        ApplicationDbContext dbContext, 
        IBoardGenerator boardGenerator,
        IHubContext<GameHub> hubContext)
    {
        _dbContext = dbContext;
        _boardGenerator = boardGenerator;
        _hubContext = hubContext;
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
            Status = "Waiting",
            IsActive = true,
            CurrentTurnUserId = userId // Creator starts first
        };

        _dbContext.Games.Add(game);
        await _dbContext.SaveChangesAsync();

        // Create game board
        var gameBoard = new GameBoard
        {
            GameId = game.Id,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.GameBoards.Add(gameBoard);
        await _dbContext.SaveChangesAsync();

        // Generate standard board cells
        var boardCells = _boardGenerator.GenerateStandardBoard(gameBoard.Id);
        _dbContext.BoardCells.AddRange(boardCells);
        await _dbContext.SaveChangesAsync();

        // Automatically add creator as first player
        var gamePlayer = new GamePlayer
        {
            GameId = game.Id,
            UserId = userId,
            PlayerOrder = 1
        };

        _dbContext.GamePlayers.Add(gamePlayer);
        await _dbContext.SaveChangesAsync();

        // Create initial player state (position 0, starting money)
        var playerState = new PlayerState
        {
            GameId = game.Id,
            UserId = userId,
            Position = 0,
            Money = 1500,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.PlayerStates.Add(playerState);
        await _dbContext.SaveChangesAsync();

        // Load the user relation
        await _dbContext.Entry(game).Reference(g => g.CreatedBy).LoadAsync();
        await _dbContext.Entry(game).Collection(g => g.GamePlayers).LoadAsync();

        return new GameResponse
        {
            Id = game.Id,
            Name = game.Name,
            PlayerCount = game.PlayerCount,
            CreatedBy = game.CreatedBy.Username,
            CreatedAt = game.CreatedAt,
            Status = game.Status,
            IsActive = game.IsActive,
            CurrentPlayerCount = game.GamePlayers.Count,
            Players = new List<string> { user.Username },
            CurrentTurnUsername = user.Username
        };
    }

    public async Task<GameResponse?> GetGameByIdAsync(int gameId)
    {
        var game = await _dbContext.Games
            .Include(g => g.CreatedBy)
            .Include(g => g.CurrentTurnUser)
            .Include(g => g.GamePlayers)
                .ThenInclude(gp => gp.User)
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
            Status = game.Status,
            IsActive = game.IsActive,
            CurrentPlayerCount = game.GamePlayers.Count,
            Players = game.GamePlayers
                .OrderBy(gp => gp.PlayerOrder)
                .Select(gp => gp.User.Username)
                .ToList(),
            CurrentTurnUsername = game.CurrentTurnUser?.Username
        };
    }

    public async Task<List<GameResponse>> GetAllGamesAsync()
    {
        var games = await _dbContext.Games
            .Include(g => g.CreatedBy)
            .Include(g => g.CurrentTurnUser)
            .Include(g => g.GamePlayers)
                .ThenInclude(gp => gp.User)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return games.Select(g => new GameResponse
        {
            Id = g.Id,
            Name = g.Name,
            PlayerCount = g.PlayerCount,
            CreatedBy = g.CreatedBy.Username,
            CreatedAt = g.CreatedAt,
            Status = g.Status,
            IsActive = g.IsActive,
            CurrentPlayerCount = g.GamePlayers.Count,
            Players = g.GamePlayers
                .OrderBy(gp => gp.PlayerOrder)
                .Select(gp => gp.User.Username)
                .ToList(),
            CurrentTurnUsername = g.CurrentTurnUser?.Username
        }).ToList();
    }

    public async Task<List<GameResponse>> GetGamesByUserAsync(int userId)
    {
        var games = await _dbContext.Games
            .Include(g => g.CreatedBy)
            .Include(g => g.CurrentTurnUser)
            .Include(g => g.GamePlayers)
                .ThenInclude(gp => gp.User)
            .Where(g => g.GamePlayers.Any(gp => gp.UserId == userId))
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return games.Select(g => new GameResponse
        {
            Id = g.Id,
            Name = g.Name,
            PlayerCount = g.PlayerCount,
            CreatedBy = g.CreatedBy.Username,
            CreatedAt = g.CreatedAt,
            Status = g.Status,
            IsActive = g.IsActive,
            CurrentPlayerCount = g.GamePlayers.Count,
            Players = g.GamePlayers
                .OrderBy(gp => gp.PlayerOrder)
                .Select(gp => gp.User.Username)
                .ToList(),
            CurrentTurnUsername = g.CurrentTurnUser?.Username
        }).ToList();
    }

    public async Task<GameResponse?> AddPlayerToGameAsync(int gameId, string username, int requestingUserId)
    {
        // Get the game with relations
        var game = await _dbContext.Games
            .Include(g => g.CreatedBy)
            .Include(g => g.GamePlayers)
                .ThenInclude(gp => gp.User)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            return null;
        }

        // Check if requesting user is the creator
        if (game.CreatedByUserId != requestingUserId)
        {
            return null; // Only creator can add players
        }

        // Check if game is full
        if (game.GamePlayers.Count >= game.PlayerCount)
        {
            return null; // Game is full
        }

        // Find user by username
        var userToAdd = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (userToAdd == null)
        {
            return null; // User not found
        }

        // Check if user is already in the game
        if (game.GamePlayers.Any(gp => gp.UserId == userToAdd.Id))
        {
            return null; // User already in game
        }

        // Get next player order
        var nextPlayerOrder = game.GamePlayers.Max(gp => gp.PlayerOrder) + 1;

        // Add player to game
        var gamePlayer = new GamePlayer
        {
            GameId = gameId,
            UserId = userToAdd.Id,
            PlayerOrder = nextPlayerOrder
        };

        _dbContext.GamePlayers.Add(gamePlayer);
        await _dbContext.SaveChangesAsync();

        // Create player state for the new player
        var playerState = new PlayerState
        {
            GameId = gameId,
            UserId = userToAdd.Id,
            Position = 0,
            Money = 1500,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.PlayerStates.Add(playerState);
        await _dbContext.SaveChangesAsync();

        // Reload game players and current turn user
        await _dbContext.Entry(game).Collection(g => g.GamePlayers).LoadAsync();
        await _dbContext.Entry(game).Reference(g => g.CurrentTurnUser).LoadAsync();

        // Return updated game response
        return new GameResponse
        {
            Id = game.Id,
            Name = game.Name,
            PlayerCount = game.PlayerCount,
            CreatedBy = game.CreatedBy.Username,
            CreatedAt = game.CreatedAt,
            Status = game.Status,
            IsActive = game.IsActive,
            CurrentPlayerCount = game.GamePlayers.Count,
            Players = game.GamePlayers
                .OrderBy(gp => gp.PlayerOrder)
                .Select(gp => gp.User.Username)
                .ToList(),
            CurrentTurnUsername = game.CurrentTurnUser?.Username
        };
    }

    public async Task<PlaygroundInfoResponse?> GetPlaygroundInfoAsync(int gameId, int userId)
    {
        // Get the game with all relations
        var game = await _dbContext.Games
            .Include(g => g.CreatedBy)
            .Include(g => g.CurrentTurnUser)
            .Include(g => g.GamePlayers)
                .ThenInclude(gp => gp.User)
            .Include(g => g.GameBoard!)
                .ThenInclude(gb => gb.BoardCells)
            .Include(g => g.PlayerStates)
                .ThenInclude(ps => ps.User)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            return null;
        }

        // Check if user has access (is a player in the game)
        var hasAccess = game.GamePlayers.Any(gp => gp.UserId == userId);
        
        if (!hasAccess)
        {
            return null; // User not authorized
        }

        // Define player colors (cycle through if more than 8 players)
        var playerColors = new[] { "#FF6B6B", "#4ECDC4", "#45B7D1", "#FFA07A", "#98D8C8", "#F7DC6F", "#BB8FCE", "#85C1E2" };
        
        // Create a map of userId to color
        var playerColorMap = game.GamePlayers
            .OrderBy(gp => gp.PlayerOrder)
            .Select((gp, index) => new { UserId = gp.UserId, Color = playerColors[index % playerColors.Length] })
            .ToDictionary(x => x.UserId, x => x.Color);

        // Build game response
        var gameResponse = new GameResponse
        {
            Id = game.Id,
            Name = game.Name,
            PlayerCount = game.PlayerCount,
            CreatedBy = game.CreatedBy.Username,
            CreatedAt = game.CreatedAt,
            Status = game.Status,
            IsActive = game.IsActive,
            CurrentPlayerCount = game.GamePlayers.Count,
            Players = game.GamePlayers
                .OrderBy(gp => gp.PlayerOrder)
                .Select(gp => gp.User.Username)
                .ToList(),
            CurrentTurnUsername = game.CurrentTurnUser?.Username
        };

        // Build board cells DTO with player positions
        var boardCells = game.GameBoard?.BoardCells
            .OrderBy(bc => bc.Position)
            .Select(bc => new BoardCellDto
            {
                Id = bc.Id,
                Position = bc.Position,
                CellType = bc.CellType,
                CellTypeName = bc.CellType.ToString(),
                Name = bc.Name,
                PlayersHere = game.PlayerStates
                    .Where(ps => ps.Position == bc.Position)
                    .Select(ps => new PlayerPositionDto
                    {
                        Username = ps.User.Username,
                        Position = ps.Position,
                        Money = ps.Money,
                        Color = playerColorMap.GetValueOrDefault(ps.UserId, "#999999")
                    })
                    .ToList()
            })
            .ToList() ?? new List<BoardCellDto>();

        return new PlaygroundInfoResponse
        {
            Game = gameResponse,
            BoardCells = boardCells,
            IsCreator = game.CreatedByUserId == userId,
            HasAccess = true
        };
    }

    public async Task<GameResponse?> EndTurnAsync(int gameId, int userId)
    {
        var game = await _dbContext.Games
            .Include(g => g.CreatedBy)
            .Include(g => g.CurrentTurnUser)
            .Include(g => g.GamePlayers)
                .ThenInclude(gp => gp.User)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            return null;
        }

        // Check if user is in the game
        var isPlayerInGame = game.GamePlayers.Any(gp => gp.UserId == userId);
        if (!isPlayerInGame)
        {
            return null;
        }

        // Check if it's this user's turn
        if (game.CurrentTurnUserId != userId)
        {
            return null;
        }

        // Get ordered list of players
        var orderedPlayers = game.GamePlayers.OrderBy(gp => gp.PlayerOrder).ToList();
        
        // Find current player index
        var currentPlayerIndex = orderedPlayers.FindIndex(gp => gp.UserId == userId);
        
        // Get next player (cycle to first if at end)
        var nextPlayerIndex = (currentPlayerIndex + 1) % orderedPlayers.Count;
        var nextPlayer = orderedPlayers[nextPlayerIndex];

        var previousUsername = game.CurrentTurnUser?.Username ?? "";

        // Update current turn
        game.CurrentTurnUserId = nextPlayer.UserId;
        await _dbContext.SaveChangesAsync();

        // Reload current turn user
        await _dbContext.Entry(game).Reference(g => g.CurrentTurnUser).LoadAsync();

        // Broadcast turn change to all players in the game
        await _hubContext.Clients.Group($"game_{gameId}").SendAsync("TurnChanged", new TurnChangedEvent
        {
            GameId = gameId,
            CurrentTurnUsername = game.CurrentTurnUser?.Username ?? "",
            PreviousTurnUsername = previousUsername
        });

        return new GameResponse
        {
            Id = game.Id,
            Name = game.Name,
            PlayerCount = game.PlayerCount,
            CreatedBy = game.CreatedBy.Username,
            CreatedAt = game.CreatedAt,
            Status = game.Status,
            IsActive = game.IsActive,
            CurrentPlayerCount = game.GamePlayers.Count,
            Players = game.GamePlayers
                .OrderBy(gp => gp.PlayerOrder)
                .Select(gp => gp.User.Username)
                .ToList(),
            CurrentTurnUsername = game.CurrentTurnUser?.Username
        };
    }

    public async Task<PlaygroundInfoResponse?> MovePlayerAsync(int gameId, int userId, int steps)
    {
        var game = await _dbContext.Games
            .Include(g => g.GamePlayers)
                .ThenInclude(gp => gp.User)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            return null;
        }

        // Check if user is in the game
        var isPlayerInGame = game.GamePlayers.Any(gp => gp.UserId == userId);
        if (!isPlayerInGame)
        {
            return null;
        }

        // Check if it's this user's turn
        if (game.CurrentTurnUserId != userId)
        {
            return null;
        }

        // Get player state
        var playerState = await _dbContext.PlayerStates
            .Include(ps => ps.User)
            .FirstOrDefaultAsync(ps => ps.GameId == gameId && ps.UserId == userId);

        if (playerState == null)
        {
            return null;
        }

        var fromPosition = playerState.Position;

        // Move player (wraparound at position 40)
        var newPosition = (playerState.Position + steps) % 40;
        
        // Check if player passed GO (position 0)
        // If old position + steps >= 40, they passed GO
        bool passedGo = (playerState.Position + steps) >= 40;
        
        Console.WriteLine($"Player {playerState.User.Username} moving: Position {playerState.Position} + {steps} steps = {playerState.Position + steps}. New position: {newPosition}. Passed GO: {passedGo}");
        
        if (passedGo)
        {
            // Give $200 for passing GO
            playerState.Money += 200;
            Console.WriteLine($"Player {playerState.User.Username} passed GO! Old money: {playerState.Money - 200}, New money: {playerState.Money}");
        }
        
        playerState.Position = newPosition;
        playerState.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        // Broadcast player moved event to all players in the game
        await _hubContext.Clients.Group($"game_{gameId}").SendAsync("PlayerMoved", new PlayerMovedEvent
        {
            GameId = gameId,
            Username = playerState.User.Username,
            FromPosition = fromPosition,
            ToPosition = playerState.Position
        });

        // Return updated playground info
        return await GetPlaygroundInfoAsync(gameId, userId);
    }

    public async Task<bool> BroadcastDiceRollAsync(int gameId, int userId, int dice1, int dice2)
    {
        var game = await _dbContext.Games
            .Include(g => g.GamePlayers)
                .ThenInclude(gp => gp.User)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            return false;
        }

        // Check if user is in the game
        var gamePlayer = game.GamePlayers.FirstOrDefault(gp => gp.UserId == userId);
        if (gamePlayer == null)
        {
            return false;
        }

        // Check if it's this user's turn
        if (game.CurrentTurnUserId != userId)
        {
            return false;
        }

        // Broadcast dice roll to all players in the game
        await _hubContext.Clients.Group($"game_{gameId}").SendAsync("DiceRolled", new DiceRolledEvent
        {
            GameId = gameId,
            Username = gamePlayer.User.Username,
            Dice1 = dice1,
            Dice2 = dice2,
            Total = dice1 + dice2
        });

        return true;
    }
}
