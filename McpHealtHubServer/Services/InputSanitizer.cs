using System.Text.RegularExpressions;
using McpHealtHubServer.Exceptions;
using Microsoft.Extensions.Logging;

namespace McpHealtHubServer.Services;

public interface IInputSanitizer
{
    string SanitizeSearchTerm(string? input);
    string SanitizeGraphQLQuery(string query);
    bool IsValidGuid(Guid id);
    bool IsValidPagination(int? page, int? pageSize);
}

public class InputSanitizer : IInputSanitizer
{
    private readonly ILogger<InputSanitizer> _logger;
    private static readonly Regex GraphQLInjectionPattern = new Regex(
        @"[\n\r\t\b\f\v]|--|\/\*|\*\/|@@|char\(|nchar\(|varchar\(|nvarchar\(|alter\s|begin\s|create\s|cursor\s|declare\s|delete\s|drop\s|end\s|exec\s|execute\s|fetch\s|insert\s|kill\s|open\s|select\s|sys\.|sysobjects|syscolumns|table\s|update\s",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex SqlInjectionPattern = new Regex(
        @"[\n\r\t\b\f\v]|--|\/\*|\*\/|@@|char\(|nchar\(|varchar\(|nvarchar\(|alter\s|begin\s|create\s|cursor\s|declare\s|delete\s|drop\s|end\s|exec\s|execute\s|fetch\s|insert\s|kill\s|open\s|select\s|sys\.|sysobjects|syscolumns|table\s|update\s",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex XssPattern = new Regex(
        @"<script|</script|<iframe|</iframe|<object|</object|<embed|</embed|<applet|</applet|<form|</form|<input|javascript:|onload=|onerror=|onclick=|onmouseover=|onfocus=|onblur=",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public InputSanitizer(ILogger<InputSanitizer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string SanitizeSearchTerm(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            _logger.LogDebug("Search term is null or empty, returning empty string");
            return string.Empty;
        }

        _logger.LogDebug("Sanitizing search term: {Input}", input);

        // Remove control characters and potential injection patterns
        var sanitized = GraphQLInjectionPattern.Replace(input, string.Empty);
        sanitized = XssPattern.Replace(sanitized, string.Empty);
        
        // Trim and limit length
        sanitized = sanitized.Trim();
        if (sanitized.Length > 200)
        {
            _logger.LogWarning("Search term exceeds maximum length of 200 characters, truncating");
            sanitized = sanitized.Substring(0, 200);
        }

        _logger.LogDebug("Sanitized search term: {Sanitized}", sanitized);
        
        return sanitized;
    }

    public string SanitizeGraphQLQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            _logger.LogWarning("GraphQL query is null or empty");
            throw new HealthHubValidationException("GraphQL query cannot be null or empty");
        }

        _logger.LogDebug("Sanitizing GraphQL query");

        // Remove control characters and potential injection patterns
        var sanitized = GraphQLInjectionPattern.Replace(query, string.Empty);
        sanitized = XssPattern.Replace(sanitized, string.Empty);
        
        // Trim whitespace
        sanitized = sanitized.Trim();

        _logger.LogDebug("GraphQL query sanitized successfully");
        
        return sanitized;
    }

    public bool IsValidGuid(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid GUID: empty GUID provided");
            return false;
        }

        _logger.LogDebug("Valid GUID: {Id}", id);
        return true;
    }

    public bool IsValidPagination(int? page, int? pageSize)
    {
        if (page.HasValue && page.Value < 1)
        {
            _logger.LogWarning("Invalid pagination: page must be greater than 0, got {Page}", page);
            return false;
        }

        if (pageSize.HasValue)
        {
            if (pageSize.Value < 1)
            {
                _logger.LogWarning("Invalid pagination: pageSize must be greater than 0, got {PageSize}", pageSize);
                return false;
            }

            if (pageSize.Value > 1000)
            {
                _logger.LogWarning("Invalid pagination: pageSize exceeds maximum allowed value of 1000, got {PageSize}", pageSize);
                return false;
            }
        }

        _logger.LogDebug("Valid pagination: page={Page}, pageSize={PageSize}", page, pageSize);
        return true;
    }

    public bool IsValidLimit(int? limit)
    {
        if (!limit.HasValue)
        {
            _logger.LogDebug("Limit is null, which is valid");
            return true;
        }

        if (limit.Value < 1)
        {
            _logger.LogWarning("Invalid limit: must be greater than 0, got {Limit}", limit);
            return false;
        }

        if (limit.Value > 10000)
        {
            _logger.LogWarning("Invalid limit: exceeds maximum allowed value of 10000, got {Limit}", limit);
            return false;
        }

        _logger.LogDebug("Valid limit: {Limit}", limit);
        return true;
    }

    public bool IsValidAgeRange(int? minAge, int? maxAge)
    {
        if (minAge.HasValue && minAge.Value < 0)
        {
            _logger.LogWarning("Invalid age range: minimum age cannot be negative, got {MinAge}", minAge);
            return false;
        }

        if (maxAge.HasValue && maxAge.Value < 0)
        {
            _logger.LogWarning("Invalid age range: maximum age cannot be negative, got {MaxAge}", maxAge);
            return false;
        }

        if (minAge.HasValue && maxAge.HasValue && minAge.Value > maxAge.Value)
        {
            _logger.LogWarning("Invalid age range: minimum age ({MinAge}) cannot be greater than maximum age ({MaxAge})", minAge, maxAge);
            return false;
        }

        _logger.LogDebug("Valid age range: minAge={MinAge}, maxAge={MaxAge}", minAge, maxAge);
        return true;
    }
}