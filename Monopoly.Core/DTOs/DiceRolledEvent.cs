namespace Monopoly.Core.DTOs;

public class DiceRolledEvent
{
    public int GameId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Dice1 { get; set; }
    public int Dice2 { get; set; }
    public int Total { get; set; }
}
