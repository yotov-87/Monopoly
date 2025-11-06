namespace Monopoly.Core.Entities;

public class GameTemplate
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ConfigurationJson { get; set; } = string.Empty; // Stores PropertyRentConfiguration as JSON
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
