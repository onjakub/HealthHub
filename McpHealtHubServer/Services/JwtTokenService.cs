using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using McpHealtHubServer.Exceptions;
using Microsoft.Extensions.Logging;

namespace McpHealtHubServer.Services;

public interface IJwtTokenService
{
    bool IsTokenValid();
    string GetToken();
    DateTime GetTokenExpiration();
    bool IsTokenExpiringSoon(int minutesBeforeExpiration = 5);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly string _token;
    private readonly JwtSecurityToken? _parsedToken;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(string token, ILogger<JwtTokenService> logger)
    {
        _token = token ?? throw new HealthHubConfigurationException("JWT token cannot be null");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(_token))
            {
                _parsedToken = handler.ReadJwtToken(_token);
                _logger.LogInformation("JWT token successfully parsed. Expiration: {Expiration}", _parsedToken.ValidTo);
            }
            else
            {
                _logger.LogWarning("JWT token cannot be read. Token may be invalid or malformed.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing JWT token");
            throw new HealthHubConfigurationException("Invalid JWT token format", ex);
        }
    }

    public bool IsTokenValid()
    {
        if (_parsedToken == null)
        {
            _logger.LogWarning("JWT token is not parsed correctly");
            return false;
        }

        var now = DateTime.UtcNow;
        var isValid = now >= _parsedToken.ValidFrom && now < _parsedToken.ValidTo;
        
        if (!isValid)
        {
            _logger.LogWarning("JWT token is invalid. Current time: {Now}, Valid from: {ValidFrom}, Valid to: {ValidTo}", 
                now, _parsedToken.ValidFrom, _parsedToken.ValidTo);
        }
        
        return isValid;
    }

    public string GetToken() => _token;

    public DateTime GetTokenExpiration()
    {
        if (_parsedToken == null)
        {
            _logger.LogWarning("JWT token is not parsed correctly, returning MinValue");
            return DateTime.MinValue;
        }
        
        return _parsedToken.ValidTo;
    }

    public bool IsTokenExpiringSoon(int minutesBeforeExpiration = 5)
    {
        if (_parsedToken == null)
        {
            _logger.LogWarning("JWT token is not parsed correctly");
            return true; // Assume expiring soon if we can't parse it
        }

        var expirationTime = _parsedToken.ValidTo;
        var warningTime = expirationTime.AddMinutes(-minutesBeforeExpiration);
        var isExpiringSoon = DateTime.UtcNow >= warningTime;
        
        if (isExpiringSoon)
        {
            _logger.LogWarning("JWT token is expiring soon. Expiration: {Expiration}, Warning time: {WarningTime}", 
                expirationTime, warningTime);
        }
        
        return isExpiringSoon;
    }

    public ClaimsPrincipal? GetClaimsPrincipal()
    {
        if (_parsedToken == null)
        {
            return null;
        }

        return new ClaimsPrincipal(new ClaimsIdentity(_parsedToken.Claims));
    }

    public string? GetClaimValue(string claimType)
    {
        if (_parsedToken == null)
        {
            return null;
        }

        return _parsedToken.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }
}