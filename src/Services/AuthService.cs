using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GeoStud.Api.Data;
using GeoStud.Api.DTOs.Auth;
using GeoStud.Api.Models;

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

    public async Task<AuthResponse?> AuthenticateUserAsync(LoginRequest request)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive && !u.IsDeleted);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                return null;
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            return new AuthResponse
            {
                AccessToken = token,
                ExpiresIn = GetTokenExpirationMinutes(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes())
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user: {Username}", request.Username);
            return null;
        }
    }

    public async Task<AuthResponse?> AuthenticateServiceAsync(ServiceAuthRequest request)
    {
        try
        {
            var serviceClient = await _context.ServiceClients
                .FirstOrDefaultAsync(sc => sc.ClientId == request.ClientId && sc.IsActive && !sc.IsDeleted);

            if (serviceClient == null || !VerifyPassword(request.ClientSecret, serviceClient.ClientSecret))
            {
                return null;
            }

            // Update last used
            serviceClient.LastUsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerateServiceJwtToken(serviceClient);
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
            return null;
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

    public Task<string?> GetUserIdFromTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return Task.FromResult(jwtToken.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value);
        }
        catch
        {
            return Task.FromResult<string?>(null);
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

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(GetJwtSecret());
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("user_id", user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim("email", user.Email),
                new Claim("type", "user")
            }),
            Expires = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
            Issuer = GetJwtIssuer(),
            Audience = GetJwtAudience(),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateServiceJwtToken(ServiceClient serviceClient)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(GetJwtSecret());
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("client_id", serviceClient.ClientId),
                new Claim("service_name", serviceClient.ServiceName),
                new Claim("type", "service")
            }),
            Expires = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
            Issuer = GetJwtIssuer(),
            Audience = GetJwtAudience(),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private bool VerifyPassword(string password, string hash)
    {
        // Simple hash verification - in production use BCrypt or similar
        return BCrypt.Net.BCrypt.Verify(password, hash);
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
