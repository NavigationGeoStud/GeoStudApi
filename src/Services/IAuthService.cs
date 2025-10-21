using GeoStud.Api.DTOs.Auth;

namespace GeoStud.Api.Services;

public interface IAuthService
{
    Task<AuthResponse?> AuthenticateServiceAsync(ServiceAuthRequest request);
    Task<bool> ValidateTokenAsync(string token);
    Task<string?> GetServiceClientIdFromTokenAsync(string token);
}
