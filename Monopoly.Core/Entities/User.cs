namespace Monopoly.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Navigation property for games the user is playing in
    public ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
    
    // Navigation property for player states
    public ICollection<PlayerState> PlayerStates { get; set; } = new List<PlayerState>();
}
