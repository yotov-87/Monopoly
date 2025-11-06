namespace Monopoly.Core.DTOs;

public class SaveGameTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<PropertyRentConfiguration> PropertyConfigurations { get; set; } = new();
}

public class GameTemplateResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GameTemplateDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<PropertyRentConfiguration> PropertyConfigurations { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
