namespace Monopoly.Core.DTOs;

public class TurnChangedEvent
{
    public int GameId { get; set; }
    public string CurrentTurnUsername { get; set; } = string.Empty;
    public string PreviousTurnUsername { get; set; } = string.Empty;
}
