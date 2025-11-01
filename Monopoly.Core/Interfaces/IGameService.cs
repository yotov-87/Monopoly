using Monopoly.Core.DTOs;

namespace Monopoly.Core.Interfaces;

public interface IGameService
{
    Task<GameResponse?> CreateGameAsync(CreateGameRequest request, int userId);
    Task<GameResponse?> GetGameByIdAsync(int gameId);
    Task<List<GameResponse>> GetAllGamesAsync();
    Task<List<GameResponse>> GetGamesByUserAsync(int userId);
}
