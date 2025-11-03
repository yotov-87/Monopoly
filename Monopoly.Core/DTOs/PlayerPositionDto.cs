namespace Monopoly.Core.DTOs;

public class PlayerPositionDto
{
    public string Username { get; set; } = string.Empty;
    public int Position { get; set; }
    public int Money { get; set; }
    public string Color { get; set; } = string.Empty; // For UI coloring
    public bool IsReady { get; set; } // For ready status
}
