using Microsoft.AspNetCore.SignalR;

namespace Monopoly.Hubs;

public class GameHub : Hub
{
    public async Task JoinGame(int gameId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"game_{gameId}");
    }

    public async Task LeaveGame(int gameId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"game_{gameId}");
    }
}
