using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GeoStud.Api.Data;
using GeoStud.Api.DTOs.Auth;
using GeoStud.Api.Models;
using GeoStud.Api.Services.Interfaces;

namespace GeoStud.Api.Services;

public class AuthService : IAuthService
{
    private readonly GeoStudDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(GeoStudDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }


    public async Task<AuthResponse?> AuthenticateServiceAsync(ServiceAuthRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting to authenticate service: {ClientId}", request.ClientId);
            
            var serviceClient = await _context.ServiceClients
                .FirstOrDefaultAsync(sc => sc.ClientId == request.ClientId && sc.IsActive && !sc.IsDeleted);

            if (serviceClient == null)
            {
                _logger.LogWarning("Service client not found or inactive: {ClientId}", request.ClientId);
                return null;
            }

            _logger.LogDebug("Service client found: {ServiceName}, verifying password...", serviceClient.ServiceName);

            if (!VerifyPassword(request.ClientSecret, serviceClient.ClientSecret))
            {
                _logger.LogWarning("Invalid password for client: {ClientId}", request.ClientId);
                return null;
            }

            _logger.LogDebug("Password verified successfully for client: {ClientId}", request.ClientId);

            // Update last used
            serviceClient.LastUsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogDebug("Generating JWT token for client: {ClientId}", request.ClientId);
            var token = GenerateServiceJwtToken(serviceClient);
            
            _logger.LogInformation("Successfully authenticated service: {ClientId}", request.ClientId);
            
            return new AuthResponse
            {
                AccessToken = token,
                ExpiresIn = GetTokenExpirationMinutes(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes())
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating service: {ClientId}", request.ClientId);
            throw; // Re-throw to trigger 500 error with details
        }
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(GetJwtSecret());

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = GetJwtIssuer(),
                ValidateAudience = true,
                ValidAudience = GetJwtAudience(),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }


    public Task<string?> GetServiceClientIdFromTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return Task.FromResult(jwtToken.Claims.FirstOrDefault(x => x.Type == "client_id")?.Value);
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }


    private string GenerateServiceJwtToken(ServiceClient serviceClient)
    {
        try
        {
            _logger.LogDebug("Starting JWT token generation for client: {ClientId}", serviceClient.ClientId);
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecret = GetJwtSecret() ?? string.Empty;
            _logger.LogDebug("JWT Secret length: {Length}", jwtSecret.Length);
            
            var key = Encoding.UTF8.GetBytes(jwtSecret);
            
            var claims = new List<Claim>
            {
                new Claim("client_id", serviceClient.ClientId ?? string.Empty),
                new Claim("type", "service")
            };
            
            if (!string.IsNullOrEmpty(serviceClient.ServiceName))
            {
                claims.Add(new Claim("service_name", serviceClient.ServiceName));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                Issuer = GetJwtIssuer(),
                Audience = GetJwtAudience(),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            
            _logger.LogDebug("JWT token generated successfully for client: {ClientId}", serviceClient.ClientId);
            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token for client: {ClientId}", serviceClient.ClientId);
            throw;
        }
    }

    private bool VerifyPassword(string password, string hash)
    {
        try
        {
            // Simple hash verification - in production use BCrypt or similar
            var result = BCrypt.Net.BCrypt.Verify(password, hash);
            _logger.LogDebug("Password verification result: {Result}", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password");
            return false;
        }
    }

    private string GetJwtSecret()
    {
        return _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
    }

    private string GetJwtIssuer()
    {
        return _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
    }

    private string GetJwtAudience()
    {
        return _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
    }

    private int GetTokenExpirationMinutes()
    {
        return _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60);
    }
}
