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
    public bool IsActive { get; set; } = true;
    
    // Current turn tracking
    public int? CurrentTurnUserId { get; set; }
    public User? CurrentTurnUser { get; set; }
    
    // Navigation property for players in the game
    public ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
    
    // Navigation property for game board
    public GameBoard? GameBoard { get; set; }
    
    // Navigation property for player states
    public ICollection<PlayerState> PlayerStates { get; set; } = new List<PlayerState>();
}
