using Monopoly.Core.Enums;

namespace Monopoly.Core.DTOs;

public class BoardCellDto
{
    public int Id { get; set; }
    public int Position { get; set; }
    public CellType CellType { get; set; }
    public string CellTypeName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<PlayerPositionDto> PlayersHere { get; set; } = new List<PlayerPositionDto>();
}
