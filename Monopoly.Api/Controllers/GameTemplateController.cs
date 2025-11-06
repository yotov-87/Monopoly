using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monopoly.Core.DTOs;
using Monopoly.Core.Interfaces;
using System.Security.Claims;

namespace Monopoly.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameTemplateController : ControllerBase
{
    private readonly IGameTemplateService _templateService;

    public GameTemplateController(IGameTemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SaveTemplate([FromBody] SaveGameTemplateRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        try
        {
            var template = await _templateService.SaveTemplateAsync(request, userId);
            return Ok(template);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetUserTemplates()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var templates = await _templateService.GetUserTemplatesAsync(userId);
        return Ok(templates);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetTemplate(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var template = await _templateService.GetTemplateByIdAsync(id, userId);
        if (template == null)
        {
            return NotFound(new { message = "Template not found." });
        }

        return Ok(template);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteTemplate(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var success = await _templateService.DeleteTemplateAsync(id, userId);
        if (!success)
        {
            return NotFound(new { message = "Template not found." });
        }

        return Ok(new { message = "Template deleted successfully." });
    }
}
