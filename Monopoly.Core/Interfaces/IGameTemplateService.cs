using Monopoly.Core.DTOs;

namespace Monopoly.Core.Interfaces;

public interface IGameTemplateService
{
    Task<GameTemplateResponse> SaveTemplateAsync(SaveGameTemplateRequest request, int userId);
    Task<List<GameTemplateResponse>> GetUserTemplatesAsync(int userId);
    Task<GameTemplateDetailResponse?> GetTemplateByIdAsync(int templateId, int userId);
    Task<bool> DeleteTemplateAsync(int templateId, int userId);
}
