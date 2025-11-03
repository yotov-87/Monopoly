using Monopoly.Core.Entities;
using Monopoly.Core.Enums;
using Monopoly.Core.Interfaces;

namespace Monopoly.Api.Services;

public class BoardGenerator : IBoardGenerator
{
    public List<BoardCell> GenerateStandardBoard(int gameBoardId)
    {
        // TODO: In the future, this can be configurable from the client
        // For now, we use the standard Monopoly board layout
        
        var cells = new List<BoardCell>
        {
            // Position 0 - Start
            new BoardCell { GameBoardId = gameBoardId, Position = 0, CellType = CellType.Start, Name = "GO" },
            
            // Position 1-9 - First side (Brown and Light Blue properties)
            new BoardCell { GameBoardId = gameBoardId, Position = 1, CellType = CellType.Property, Name = "Mediterranean Avenue", ColorGroup = "Brown" },
            new BoardCell { GameBoardId = gameBoardId, Position = 2, CellType = CellType.CommunityChest, Name = "Community Chest" },
            new BoardCell { GameBoardId = gameBoardId, Position = 3, CellType = CellType.Property, Name = "Baltic Avenue", ColorGroup = "Brown" },
            new BoardCell { GameBoardId = gameBoardId, Position = 4, CellType = CellType.Tax, Name = "Income Tax" },
            new BoardCell { GameBoardId = gameBoardId, Position = 5, CellType = CellType.Railroad, Name = "Reading Railroad" },
            new BoardCell { GameBoardId = gameBoardId, Position = 6, CellType = CellType.Property, Name = "Oriental Avenue", ColorGroup = "LightBlue" },
            new BoardCell { GameBoardId = gameBoardId, Position = 7, CellType = CellType.Chance, Name = "Chance" },
            new BoardCell { GameBoardId = gameBoardId, Position = 8, CellType = CellType.Property, Name = "Vermont Avenue", ColorGroup = "LightBlue" },
            new BoardCell { GameBoardId = gameBoardId, Position = 9, CellType = CellType.Property, Name = "Connecticut Avenue", ColorGroup = "LightBlue" },
            
            // Position 10 - Jail
            new BoardCell { GameBoardId = gameBoardId, Position = 10, CellType = CellType.Jail, Name = "Jail / Just Visiting" },
            
            // Position 11-19 - Second side (Pink and Orange properties)
            new BoardCell { GameBoardId = gameBoardId, Position = 11, CellType = CellType.Property, Name = "St. Charles Place", ColorGroup = "Pink" },
            new BoardCell { GameBoardId = gameBoardId, Position = 12, CellType = CellType.Utility, Name = "Electric Company" },
            new BoardCell { GameBoardId = gameBoardId, Position = 13, CellType = CellType.Property, Name = "States Avenue", ColorGroup = "Pink" },
            new BoardCell { GameBoardId = gameBoardId, Position = 14, CellType = CellType.Property, Name = "Virginia Avenue", ColorGroup = "Pink" },
            new BoardCell { GameBoardId = gameBoardId, Position = 15, CellType = CellType.Railroad, Name = "Pennsylvania Railroad" },
            new BoardCell { GameBoardId = gameBoardId, Position = 16, CellType = CellType.Property, Name = "St. James Place", ColorGroup = "Orange" },
            new BoardCell { GameBoardId = gameBoardId, Position = 17, CellType = CellType.CommunityChest, Name = "Community Chest" },
            new BoardCell { GameBoardId = gameBoardId, Position = 18, CellType = CellType.Property, Name = "Tennessee Avenue", ColorGroup = "Orange" },
            new BoardCell { GameBoardId = gameBoardId, Position = 19, CellType = CellType.Property, Name = "New York Avenue", ColorGroup = "Orange" },
            
            // Position 20 - Free Parking
            new BoardCell { GameBoardId = gameBoardId, Position = 20, CellType = CellType.FreeParking, Name = "Free Parking" },
            
            // Position 21-29 - Third side (Red and Yellow properties)
            new BoardCell { GameBoardId = gameBoardId, Position = 21, CellType = CellType.Property, Name = "Kentucky Avenue", ColorGroup = "Red" },
            new BoardCell { GameBoardId = gameBoardId, Position = 22, CellType = CellType.Chance, Name = "Chance" },
            new BoardCell { GameBoardId = gameBoardId, Position = 23, CellType = CellType.Property, Name = "Indiana Avenue", ColorGroup = "Red" },
            new BoardCell { GameBoardId = gameBoardId, Position = 24, CellType = CellType.Property, Name = "Illinois Avenue", ColorGroup = "Red" },
            new BoardCell { GameBoardId = gameBoardId, Position = 25, CellType = CellType.Railroad, Name = "B. & O. Railroad" },
            new BoardCell { GameBoardId = gameBoardId, Position = 26, CellType = CellType.Property, Name = "Atlantic Avenue", ColorGroup = "Yellow" },
            new BoardCell { GameBoardId = gameBoardId, Position = 27, CellType = CellType.Property, Name = "Ventnor Avenue", ColorGroup = "Yellow" },
            new BoardCell { GameBoardId = gameBoardId, Position = 28, CellType = CellType.Utility, Name = "Water Works" },
            new BoardCell { GameBoardId = gameBoardId, Position = 29, CellType = CellType.Property, Name = "Marvin Gardens", ColorGroup = "Yellow" },
            
            // Position 30 - Go To Jail
            new BoardCell { GameBoardId = gameBoardId, Position = 30, CellType = CellType.GoToJail, Name = "Go To Jail" },
            
            // Position 31-39 - Fourth side (Green and Dark Blue properties)
            new BoardCell { GameBoardId = gameBoardId, Position = 31, CellType = CellType.Property, Name = "Pacific Avenue", ColorGroup = "Green" },
            new BoardCell { GameBoardId = gameBoardId, Position = 32, CellType = CellType.Property, Name = "North Carolina Avenue", ColorGroup = "Green" },
            new BoardCell { GameBoardId = gameBoardId, Position = 33, CellType = CellType.CommunityChest, Name = "Community Chest" },
            new BoardCell { GameBoardId = gameBoardId, Position = 34, CellType = CellType.Property, Name = "Pennsylvania Avenue", ColorGroup = "Green" },
            new BoardCell { GameBoardId = gameBoardId, Position = 35, CellType = CellType.Railroad, Name = "Short Line Railroad" },
            new BoardCell { GameBoardId = gameBoardId, Position = 36, CellType = CellType.Chance, Name = "Chance" },
            new BoardCell { GameBoardId = gameBoardId, Position = 37, CellType = CellType.Property, Name = "Park Place", ColorGroup = "DarkBlue" },
            new BoardCell { GameBoardId = gameBoardId, Position = 38, CellType = CellType.Tax, Name = "Luxury Tax" },
            new BoardCell { GameBoardId = gameBoardId, Position = 39, CellType = CellType.Property, Name = "Boardwalk", ColorGroup = "DarkBlue" }
        };
        
        return cells;
    }
}
