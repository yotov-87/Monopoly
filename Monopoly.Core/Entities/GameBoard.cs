namespace Monopoly.Core.Entities;

public class GameBoard
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    
    // Navigation property for board cells
    public ICollection<BoardCell> BoardCells { get; set; } = new List<BoardCell>();
}
