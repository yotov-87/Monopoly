using Monopoly.Core.Enums;

namespace Monopoly.Core.Entities;

public class BoardCell
{
    public int Id { get; set; }
    public int GameBoardId { get; set; }
    public GameBoard GameBoard { get; set; } = null!;
    public int Position { get; set; } // Position on the board (0-39 for standard Monopoly)
    public CellType CellType { get; set; }
    public string Name { get; set; } = string.Empty;
}
