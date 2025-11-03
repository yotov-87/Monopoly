namespace Monopoly.Core.DTOs;

public class PlayerReadyEvent
{
    public int GameId { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsReady { get; set; }
    public bool AllPlayersReady { get; set; }
}
