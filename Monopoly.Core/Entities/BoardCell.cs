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
    public string? ColorGroup { get; set; } // Color group for properties (Brown, LightBlue, Pink, etc.)
    public int? Price { get; set; } // Purchase price for properties
    public int? Rent { get; set; } // Rent amount for properties (default 100)
    public int Houses { get; set; } = 0; // Number of houses built on this property (0-4)
    public int? OwnerId { get; set; } // GamePlayer.Id who owns this property
    public GamePlayer? Owner { get; set; } // Navigation property
}
