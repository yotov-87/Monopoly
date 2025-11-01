namespace Monopoly.Core.DTOs;

public class GameResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int PlayerCount { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
