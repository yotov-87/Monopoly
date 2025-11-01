namespace Monopoly.Core.Entities;

public class Game
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int PlayerCount { get; set; }
    public int CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "Waiting"; // Waiting, InProgress, Completed
}
