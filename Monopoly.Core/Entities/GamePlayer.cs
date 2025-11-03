namespace Monopoly.Core.Entities;

public class GamePlayer
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int PlayerOrder { get; set; } // Order in which player joined (1, 2, 3, etc.)
}
