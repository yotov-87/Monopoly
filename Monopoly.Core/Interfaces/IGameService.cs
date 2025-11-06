using Monopoly.Core.DTOs;
using Monopoly.Core.Entities;

namespace Monopoly.Core.Interfaces;

public interface IGameService
{
    Task<GameResponse?> CreateGameAsync(CreateGameRequest request, int userId);
    Task<GameResponse?> GetGameByIdAsync(int id);
    Task<List<GameResponse>> GetAllGamesAsync();
    Task<List<GameResponse>> GetGamesByUserAsync(int userId);
    Task<GameResponse?> AddPlayerToGameAsync(int gameId, string username, int requestingUserId);
    Task<PlaygroundInfoResponse?> GetPlaygroundInfoAsync(int gameId, int userId);
    Task<GameResponse?> EndTurnAsync(int gameId, int userId);
    Task<PlaygroundInfoResponse?> MovePlayerAsync(int gameId, int userId, int steps);
    Task<bool> BroadcastDiceRollAsync(int gameId, int userId, int dice1, int dice2);
    Task<bool> SetPlayerReadyAsync(int gameId, int userId, bool isReady);
    Task<PlaygroundInfoResponse?> PurchasePropertyAsync(int gameId, int userId, int cellId);
    Task<PlaygroundInfoResponse?> PayRentAsync(int gameId, int userId, int cellId);
    Task<int?> ProposeTradeAsync(int gameId, int userId, int cellId, int offeredPrice);
    Task<PlaygroundInfoResponse?> RespondToTradeAsync(int gameId, int userId, int tradeId, bool accept);
    Task<PlaygroundInfoResponse?> BuildHouseAsync(int gameId, int userId, int cellId);
    Task<PlaygroundInfoResponse?> BuildHotelAsync(int gameId, int userId, int cellId);
}
