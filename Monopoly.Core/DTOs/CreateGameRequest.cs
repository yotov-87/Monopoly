namespace Monopoly.Core.DTOs;

public class CreateGameRequest
{
    public string Name { get; set; } = string.Empty;
    public int PlayerCount { get; set; }
}
