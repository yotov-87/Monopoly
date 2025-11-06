using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Monopoly.Core.DTOs;
using Monopoly.Core.Entities;
using Monopoly.Core.Enums;
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
        
        // Apply custom property configurations if provided
        if (request.CustomRents != null && request.CustomRents.Any())
        {
            foreach (var customConfig in request.CustomRents)
            {
                var cell = boardCells.FirstOrDefault(c => c.Position == customConfig.Position);
                if (cell != null && (cell.CellType == CellType.Property || cell.CellType == CellType.Railroad || cell.CellType == CellType.Utility))
                {
                    cell.Price = customConfig.Price;
                    cell.Rent = customConfig.Rent;
                    // Note: HousePrice and HotelPrice can be added to BoardCell entity in the future
                    // For now, we only apply Price and Rent
                }
            }
        }
        
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
            IsReady = false, // Explicitly set to false
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
            IsReady = false, // Explicitly set to false
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.PlayerStates.Add(playerState);
        await _dbContext.SaveChangesAsync();

        // Reload game players and current turn user
        await _dbContext.Entry(game).Collection(g => g.GamePlayers).LoadAsync();
        await _dbContext.Entry(game).Reference(g => g.CurrentTurnUser).LoadAsync();

        // Broadcast PlayerJoined event via SignalR
        var playerJoinedEvent = new PlayerJoinedEvent
        {
            GameId = gameId,
            Username = userToAdd.Username,
            CurrentPlayerCount = game.GamePlayers.Count,
            MaxPlayerCount = game.PlayerCount
        };

        await _hubContext.Clients.Group($"game_{gameId}").SendAsync("PlayerJoined", playerJoinedEvent);

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
                ColorGroup = bc.ColorGroup,
                Price = bc.Price,
                Houses = bc.Houses,
                OwnerUsername = bc.OwnerId.HasValue 
                    ? game.GamePlayers.FirstOrDefault(gp => gp.Id == bc.OwnerId.Value)?.User.Username 
                    : null,
                PlayersHere = game.PlayerStates
                    .Where(ps => ps.Position == bc.Position)
                    .Select(ps => new PlayerPositionDto
                    {
                        Username = ps.User.Username,
                        Position = ps.Position,
                        Money = ps.Money,
                        Color = playerColorMap.GetValueOrDefault(ps.UserId, "#999999"),
                        IsReady = ps.IsReady
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

    public async Task<bool> SetPlayerReadyAsync(int gameId, int userId, bool isReady)
    {
        var game = await _dbContext.Games
            .Include(g => g.GamePlayers)
                .ThenInclude(gp => gp.User)
            .Include(g => g.PlayerStates)
                .ThenInclude(ps => ps.User)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            return false;
        }

        // Check if user is in the game
        var isPlayerInGame = game.GamePlayers.Any(gp => gp.UserId == userId);
        if (!isPlayerInGame)
        {
            return false;
        }

        // Update player ready status
        var playerState = await _dbContext.PlayerStates
            .Include(ps => ps.User)
            .FirstOrDefaultAsync(ps => ps.GameId == gameId && ps.UserId == userId);

        if (playerState == null)
        {
            return false;
        }

        playerState.IsReady = isReady;
        playerState.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        // Check if all players are ready
        var allPlayersReady = game.PlayerStates.All(ps => ps.IsReady);

        // Broadcast player ready status to all players in the game
        await _hubContext.Clients.Group($"game_{gameId}").SendAsync("PlayerReady", new PlayerReadyEvent
        {
            GameId = gameId,
            Username = playerState.User.Username,
            IsReady = isReady,
            AllPlayersReady = allPlayersReady
        });

        // If all players are ready and game is Waiting, change status to Active
        if (allPlayersReady && game.Status == "Waiting")
        {
            game.Status = "Active";
            await _dbContext.SaveChangesAsync();

            // Broadcast game started event
            await _hubContext.Clients.Group($"game_{gameId}").SendAsync("GameStarted", new
            {
                GameId = gameId,
                Status = game.Status
            });
        }

        return true;
    }

    public async Task<PlaygroundInfoResponse?> PurchasePropertyAsync(int gameId, int userId, int cellId)
    {
        var game = await _dbContext.Games
            .Include(g => g.GameBoard!)
                .ThenInclude(gb => gb.BoardCells)
            .Include(g => g.GamePlayers)
                .ThenInclude(gp => gp.User)
            .Include(g => g.PlayerStates)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            throw new InvalidOperationException("Game not found");
        }

        // Find the player
        var gamePlayer = game.GamePlayers.FirstOrDefault(gp => gp.UserId == userId);
        if (gamePlayer == null)
        {
            throw new InvalidOperationException("Player not in game");
        }

        // Find player state
        var playerState = game.PlayerStates.FirstOrDefault(ps => ps.UserId == userId);
        if (playerState == null)
        {
            throw new InvalidOperationException("Player state not found");
        }

        // Find the cell
        var cell = game.GameBoard?.BoardCells.FirstOrDefault(c => c.Id == cellId);
        if (cell == null)
        {
            throw new InvalidOperationException("Cell not found");
        }

        // Validate purchase conditions
        if (cell.CellType != CellType.Property && cell.CellType != CellType.Railroad && cell.CellType != CellType.Utility)
        {
            throw new InvalidOperationException("This cell cannot be purchased");
        }

        if (cell.OwnerId.HasValue)
        {
            throw new InvalidOperationException("Property already owned");
        }

        if (!cell.Price.HasValue)
        {
            throw new InvalidOperationException("Property has no price");
        }

        if (playerState.Money < cell.Price.Value)
        {
            throw new InvalidOperationException("Not enough money");
        }

        // Check if player is on this cell
        if (playerState.Position != cell.Position)
        {
            throw new InvalidOperationException("Player not on this cell");
        }

        // Purchase the property
        cell.OwnerId = gamePlayer.Id;
        playerState.Money -= cell.Price.Value;

        await _dbContext.SaveChangesAsync();

        // Broadcast property purchased event
        await _hubContext.Clients.Group($"game_{gameId}").SendAsync("PropertyPurchased", new
        {
            GameId = gameId,
            CellId = cellId,
            OwnerUsername = gamePlayer.User.Username,
            Price = cell.Price.Value
        });

                // Return updated playground info
        return await GetPlaygroundInfoAsync(gameId, userId);
    }

    public async Task<PlaygroundInfoResponse?> PayRentAsync(int gameId, int userId, int cellId)
    {
        var game = await _dbContext.Games
            .Include(g => g.GameBoard!)
                .ThenInclude(gb => gb.BoardCells)
            .Include(g => g.GamePlayers)
                .ThenInclude(gp => gp.User)
            .Include(g => g.PlayerStates)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            throw new InvalidOperationException("Game not found");
        }

        // Find the current player (tenant)
        var tenantPlayer = game.GamePlayers.FirstOrDefault(gp => gp.UserId == userId);
        if (tenantPlayer == null)
        {
            throw new InvalidOperationException("Player not in game");
        }

        var tenantState = game.PlayerStates.FirstOrDefault(ps => ps.UserId == userId);
        if (tenantState == null)
        {
            throw new InvalidOperationException("Player state not found");
        }

        // Find the cell
        var cell = game.GameBoard?.BoardCells.FirstOrDefault(c => c.Id == cellId);
        if (cell == null)
        {
            throw new InvalidOperationException("Cell not found");
        }

        // Validate rent payment conditions
        if (cell.CellType != CellType.Property && cell.CellType != CellType.Railroad && cell.CellType != CellType.Utility)
        {
            throw new InvalidOperationException("This cell does not require rent");
        }

        if (!cell.OwnerId.HasValue)
        {
            throw new InvalidOperationException("Property is not owned");
        }

        // Find the owner
        var ownerPlayer = game.GamePlayers.FirstOrDefault(gp => gp.Id == cell.OwnerId.Value);
        if (ownerPlayer == null)
        {
            throw new InvalidOperationException("Owner not found");
        }

        // Check if player is trying to pay rent to themselves
        if (ownerPlayer.UserId == userId)
        {
            throw new InvalidOperationException("Cannot pay rent to yourself");
        }

        var ownerState = game.PlayerStates.FirstOrDefault(ps => ps.UserId == ownerPlayer.UserId);
        if (ownerState == null)
        {
            throw new InvalidOperationException("Owner state not found");
        }

        // Calculate rent: check if owner has monopoly (all properties of same color) and houses
        int baseRent = cell.Rent ?? 100; // Use cell's rent value or default to 100
        int rentAmount = baseRent;
        
        // If property has houses, rent increases by base rent for each house
        // 0 houses = baseRent, 1 house = baseRent * 2, 2 houses = baseRent * 3, etc.
        if (cell.Houses > 0)
        {
            rentAmount = baseRent * (cell.Houses + 1);
        }
        else if (!string.IsNullOrEmpty(cell.ColorGroup))
        {
            // Get all properties in the same color group
            var propertiesInGroup = game.GameBoard?.BoardCells
                .Where(c => c.CellType == CellType.Property && c.ColorGroup == cell.ColorGroup)
                .ToList() ?? new List<BoardCell>();

            // Check if owner owns all properties in this color group
            var ownerOwnsAll = propertiesInGroup.All(c => c.OwnerId == ownerPlayer.Id);

            if (ownerOwnsAll && propertiesInGroup.Count > 0)
            {
                // Monopoly without houses! Rent is 2x (changed from 5x to match standard rules)
                rentAmount = baseRent * 2;
            }
        }

        // Check if tenant has enough money
        if (tenantState.Money < rentAmount)
        {
            throw new InvalidOperationException("Not enough money to pay rent");
        }

        // Check if player is on this cell
        if (tenantState.Position != cell.Position)
        {
            throw new InvalidOperationException("Player not on this cell");
        }

        // Transfer rent
        tenantState.Money -= rentAmount;
        ownerState.Money += rentAmount;

        await _dbContext.SaveChangesAsync();

        // Broadcast rent paid event
        await _hubContext.Clients.Group($"game_{gameId}").SendAsync("RentPaid", new
        {
            GameId = gameId,
            CellId = cellId,
            TenantUsername = tenantPlayer.User.Username,
            OwnerUsername = ownerPlayer.User.Username,
            Amount = rentAmount,
            IsMonopoly = rentAmount > baseRent
        });

        // Return updated playground info
        return await GetPlaygroundInfoAsync(gameId, userId);
    }

    public async Task<int?> ProposeTradeAsync(int gameId, int userId, int cellId, int offeredPrice)
    {
        var game = await _dbContext.Games
            .Include(g => g.GameBoard!)
                .ThenInclude(gb => gb.BoardCells)
            .Include(g => g.GamePlayers)
                .ThenInclude(gp => gp.User)
            .Include(g => g.PlayerStates)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            throw new InvalidOperationException("Game not found");
        }

        // Find buyer player
        var buyerPlayer = game.GamePlayers.FirstOrDefault(gp => gp.UserId == userId);
        if (buyerPlayer == null)
        {
            throw new InvalidOperationException("Player not in game");
        }

        var buyerState = game.PlayerStates.FirstOrDefault(ps => ps.UserId == userId);
        if (buyerState == null)
        {
            throw new InvalidOperationException("Player state not found");
        }

        // Find the cell
        var cell = game.GameBoard?.BoardCells.FirstOrDefault(c => c.Id == cellId);
        if (cell == null)
        {
            throw new InvalidOperationException("Cell not found");
        }

        // Validate trade conditions
        if (cell.CellType != CellType.Property && cell.CellType != CellType.Railroad && cell.CellType != CellType.Utility)
        {
            throw new InvalidOperationException("This cell cannot be traded");
        }

        if (!cell.OwnerId.HasValue)
        {
            throw new InvalidOperationException("Property is not owned");
        }

        // Find the owner
        var sellerPlayer = game.GamePlayers.FirstOrDefault(gp => gp.Id == cell.OwnerId.Value);
        if (sellerPlayer == null)
        {
            throw new InvalidOperationException("Owner not found");
        }

        // Cannot trade with yourself
        if (sellerPlayer.UserId == userId)
        {
            throw new InvalidOperationException("Cannot trade with yourself");
        }

        // Check if buyer has enough money
        if (buyerState.Money < offeredPrice)
        {
            throw new InvalidOperationException("Not enough money for this offer");
        }

        // Check for existing pending trades for this property
        var existingTrade = await _dbContext.PropertyTrades
            .FirstOrDefaultAsync(pt => pt.GameId == gameId 
                && pt.CellPosition == cell.Position 
                && pt.Status == TradeStatus.Pending);

        if (existingTrade != null)
        {
            throw new InvalidOperationException("There is already a pending trade for this property");
        }

        // Create trade offer
        var trade = new PropertyTrade
        {
            GameId = gameId,
            CellPosition = cell.Position,
            BuyerPlayerId = buyerPlayer.Id,
            SellerPlayerId = sellerPlayer.Id,
            OfferedPrice = offeredPrice,
            Status = TradeStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.PropertyTrades.Add(trade);
        await _dbContext.SaveChangesAsync();

        // Broadcast trade proposal to owner
        await _hubContext.Clients.Group($"game_{gameId}").SendAsync("TradeProposed", new
        {
            TradeId = trade.Id,
            GameId = gameId,
            BuyerUsername = buyerPlayer.User.Username,
            SellerUsername = sellerPlayer.User.Username,
            CellPosition = cell.Position,
            CellName = cell.Name,
            OfferedPrice = offeredPrice
        });

        return trade.Id;
    }

    public async Task<PlaygroundInfoResponse?> RespondToTradeAsync(int gameId, int userId, int tradeId, bool accept)
    {
        var trade = await _dbContext.PropertyTrades
            .Include(t => t.BuyerPlayer)
                .ThenInclude(bp => bp.User)
            .Include(t => t.SellerPlayer)
                .ThenInclude(sp => sp.User)
            .FirstOrDefaultAsync(t => t.Id == tradeId && t.GameId == gameId);

        if (trade == null)
        {
            throw new InvalidOperationException("Trade not found");
        }

        // Verify the user is the seller
        if (trade.SellerPlayer.UserId != userId)
        {
            throw new InvalidOperationException("Only the property owner can respond to this trade");
        }

        // Check if trade is still pending
        if (trade.Status != TradeStatus.Pending)
        {
            throw new InvalidOperationException("Trade has already been responded to");
        }

        var game = await _dbContext.Games
            .Include(g => g.GameBoard!)
                .ThenInclude(gb => gb.BoardCells)
            .Include(g => g.PlayerStates)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            throw new InvalidOperationException("Game not found");
        }

        // Find the cell
        var cell = game.GameBoard?.BoardCells.FirstOrDefault(c => c.Position == trade.CellPosition);
        if (cell == null)
        {
            throw new InvalidOperationException("Cell not found");
        }

        if (accept)
        {
            // Verify ownership hasn't changed
            if (cell.OwnerId != trade.SellerPlayer.Id)
            {
                throw new InvalidOperationException("Property ownership has changed");
            }

            // Find player states
            var buyerState = game.PlayerStates.FirstOrDefault(ps => ps.UserId == trade.BuyerPlayer.UserId);
            var sellerState = game.PlayerStates.FirstOrDefault(ps => ps.UserId == trade.SellerPlayer.UserId);

            if (buyerState == null || sellerState == null)
            {
                throw new InvalidOperationException("Player states not found");
            }

            // Verify buyer still has enough money
            if (buyerState.Money < trade.OfferedPrice)
            {
                throw new InvalidOperationException("Buyer no longer has enough money");
            }

            // Execute trade: transfer ownership and money
            cell.OwnerId = trade.BuyerPlayer.Id;
            buyerState.Money -= trade.OfferedPrice;
            sellerState.Money += trade.OfferedPrice;

            trade.Status = TradeStatus.Accepted;
            trade.RespondedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Broadcast trade accepted
            await _hubContext.Clients.Group($"game_{gameId}").SendAsync("TradeAccepted", new
            {
                TradeId = trade.Id,
                GameId = gameId,
                BuyerUsername = trade.BuyerPlayer.User.Username,
                SellerUsername = trade.SellerPlayer.User.Username,
                CellPosition = cell.Position,
                CellName = cell.Name,
                Price = trade.OfferedPrice
            });
        }
        else
        {
            // Trade rejected
            trade.Status = TradeStatus.Rejected;
            trade.RespondedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Broadcast trade rejected
            await _hubContext.Clients.Group($"game_{gameId}").SendAsync("TradeRejected", new
            {
                TradeId = trade.Id,
                GameId = gameId,
                BuyerUsername = trade.BuyerPlayer.User.Username,
                SellerUsername = trade.SellerPlayer.User.Username,
                CellPosition = cell.Position,
                CellName = cell.Name
            });
        }

        // Return updated playground info
        return await GetPlaygroundInfoAsync(gameId, userId);
    }

    public async Task<PlaygroundInfoResponse?> BuildHouseAsync(int gameId, int userId, int cellId)
    {
        var game = await _dbContext.Games
            .Include(g => g.GameBoard!)
                .ThenInclude(gb => gb.BoardCells)
            .Include(g => g.GamePlayers)
                .ThenInclude(gp => gp.User)
            .Include(g => g.PlayerStates)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            throw new InvalidOperationException("Game not found");
        }

        // Find the player
        var gamePlayer = game.GamePlayers.FirstOrDefault(gp => gp.UserId == userId);
        if (gamePlayer == null)
        {
            throw new InvalidOperationException("Player not in game");
        }

        // Find player state
        var playerState = game.PlayerStates.FirstOrDefault(ps => ps.UserId == userId);
        if (playerState == null)
        {
            throw new InvalidOperationException("Player state not found");
        }

        // Find the cell
        var cell = game.GameBoard?.BoardCells.FirstOrDefault(c => c.Id == cellId);
        if (cell == null)
        {
            throw new InvalidOperationException("Cell not found");
        }

        // Validate building conditions
        if (cell.CellType != CellType.Property)
        {
            throw new InvalidOperationException("Can only build houses on properties");
        }

        if (cell.OwnerId != gamePlayer.Id)
        {
            throw new InvalidOperationException("You don't own this property");
        }

        if (string.IsNullOrEmpty(cell.ColorGroup))
        {
            throw new InvalidOperationException("Property has no color group");
        }

        // Check if player owns all properties in this color group (monopoly)
        var propertiesInGroup = game.GameBoard?.BoardCells
            .Where(c => c.CellType == CellType.Property && c.ColorGroup == cell.ColorGroup)
            .ToList() ?? new List<BoardCell>();

        var ownsMonopoly = propertiesInGroup.All(c => c.OwnerId == gamePlayer.Id);
        if (!ownsMonopoly)
        {
            throw new InvalidOperationException("You must own all properties in this color group to build houses");
        }

        // Check if already has 4 houses
        if (cell.Houses >= 4)
        {
            throw new InvalidOperationException("Maximum 4 houses per property");
        }

        // Check balanced building rule: can't build if this property would have 2 more houses than any other in the group
        var minHousesInGroup = propertiesInGroup.Min(c => c.Houses);
        if (cell.Houses > minHousesInGroup)
        {
            throw new InvalidOperationException("Must build houses evenly across all properties in the color group");
        }

        // Get house price from PropertyRentConfiguration (stored during game creation)
        // For now, use a default price of 50 per house
        int housePrice = 50; // TODO: Get from game configuration

        // Check if player has enough money
        if (playerState.Money < housePrice)
        {
            throw new InvalidOperationException("Not enough money to build a house");
        }

        // Build the house
        cell.Houses++;
        playerState.Money -= housePrice;

        await _dbContext.SaveChangesAsync();

        // Broadcast house built event
        await _hubContext.Clients.Group($"game_{gameId}").SendAsync("HouseBuilt", new
        {
            GameId = gameId,
            CellId = cellId,
            CellName = cell.Name,
            OwnerUsername = gamePlayer.User.Username,
            Houses = cell.Houses,
            Price = housePrice
        });

        // Return updated playground info
        return await GetPlaygroundInfoAsync(gameId, userId);
    }
}

