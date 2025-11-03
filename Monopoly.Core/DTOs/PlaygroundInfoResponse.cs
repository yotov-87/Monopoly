namespace Monopoly.Core.DTOs;

public class PlaygroundInfoResponse
{
    public GameResponse Game { get; set; } = new GameResponse();
    public List<BoardCellDto> BoardCells { get; set; } = new List<BoardCellDto>();
    public bool IsCreator { get; set; }
    public bool HasAccess { get; set; }
}
