using Monopoly.Core.DTOs;
using Monopoly.Core.Entities;

namespace Monopoly.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    string GenerateJwtToken(User user);
}
