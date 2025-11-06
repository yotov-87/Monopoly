namespace Monopoly.Core.DTOs;

public class PlayerJoinedEvent
{
    public int GameId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int CurrentPlayerCount { get; set; }
    public int MaxPlayerCount { get; set; }
}
