namespace Monopoly.Core.Entities;

public class PlayerState
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int UserId { get; set; }
    public int Position { get; set; } = 0; // 0-39
    public int Money { get; set; } = 1500; // Starting money
    public bool IsInJail { get; set; } = false;
    public int JailTurns { get; set; } = 0;
    public bool IsReady { get; set; } = false; // Player ready status
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Game Game { get; set; } = null!;
    public User User { get; set; } = null!;
}
