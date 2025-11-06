using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Monopoly.Core.DTOs;
using Monopoly.Core.Entities;
using Monopoly.Core.Interfaces;
using Monopoly.Data;

namespace Monopoly.Api.Services;

public class GameTemplateService : IGameTemplateService
{
    private readonly ApplicationDbContext _dbContext;

    public GameTemplateService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GameTemplateResponse> SaveTemplateAsync(SaveGameTemplateRequest request, int userId)
    {
        // Serialize property configurations to JSON
        var configJson = JsonSerializer.Serialize(request.PropertyConfigurations);

        var template = new GameTemplate
        {
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            ConfigurationJson = configJson,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.GameTemplates.Add(template);
        await _dbContext.SaveChangesAsync();

        return new GameTemplateResponse
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            CreatedAt = template.CreatedAt
        };
    }

    public async Task<List<GameTemplateResponse>> GetUserTemplatesAsync(int userId)
    {
        return await _dbContext.GameTemplates
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new GameTemplateResponse
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<GameTemplateDetailResponse?> GetTemplateByIdAsync(int templateId, int userId)
    {
        var template = await _dbContext.GameTemplates
            .FirstOrDefaultAsync(t => t.Id == templateId && t.UserId == userId);

        if (template == null)
            return null;

        // Deserialize JSON back to property configurations
        var propertyConfigurations = JsonSerializer.Deserialize<List<PropertyRentConfiguration>>(template.ConfigurationJson)
            ?? new List<PropertyRentConfiguration>();

        return new GameTemplateDetailResponse
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            PropertyConfigurations = propertyConfigurations,
            CreatedAt = template.CreatedAt
        };
    }

    public async Task<bool> DeleteTemplateAsync(int templateId, int userId)
    {
        var template = await _dbContext.GameTemplates
            .FirstOrDefaultAsync(t => t.Id == templateId && t.UserId == userId);

        if (template == null)
            return false;

        _dbContext.GameTemplates.Remove(template);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
