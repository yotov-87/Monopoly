using Monopoly.Core.Enums;

namespace Monopoly.Core.DTOs;

public class BoardCellDto
{
    public int Id { get; set; }
    public int Position { get; set; }
    public CellType CellType { get; set; }
    public string CellTypeName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ColorGroup { get; set; }
    public int? Price { get; set; }
    public int? Rent { get; set; }
    public string? OwnerUsername { get; set; }
    public List<PlayerPositionDto> PlayersHere { get; set; } = new List<PlayerPositionDto>();
}
