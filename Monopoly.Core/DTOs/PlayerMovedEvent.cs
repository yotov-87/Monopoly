namespace Monopoly.Core.DTOs;

public class PlayerMovedEvent
{
    public int GameId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int FromPosition { get; set; }
    public int ToPosition { get; set; }
}
