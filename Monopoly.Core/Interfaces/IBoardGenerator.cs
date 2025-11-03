using Monopoly.Core.Entities;

namespace Monopoly.Core.Interfaces;

public interface IBoardGenerator
{
    List<BoardCell> GenerateStandardBoard(int gameBoardId);
}
