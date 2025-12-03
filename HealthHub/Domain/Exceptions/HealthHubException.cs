using System;

namespace HealthHub.Domain.Exceptions;

/// <summary>
/// Base exception for domain-specific business logic errors
/// </summary>
public class HealthHubException : Exception
{
    public string ErrorCode { get; }
    public string? Details { get; }

    public HealthHubException(string message, string errorCode, string? details = null)
        : base(message)
    {
        ErrorCode = errorCode;
        Details = details;
    }

    public HealthHubException(string message, string errorCode, Exception innerException, string? details = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Details = details;
    }
}

/// <summary>
/// Exception for validation errors
/// </summary>
public class ValidationException : HealthHubException
{
    public ValidationException(string message, string? details = null)
        : base(message, "VALIDATION_ERROR", details) { }
}

/// <summary>
/// Exception for not found errors
/// </summary>
public class NotFoundException : HealthHubException
{
    public NotFoundException(string message, string? details = null)
        : base(message, "NOT_FOUND", details) { }
}

/// <summary>
/// Exception for unauthorized access
/// </summary>
public class UnauthorizedException : HealthHubException
{
    public UnauthorizedException(string message, string? details = null)
        : base(message, "UNAUTHORIZED", details) { }
}

/// <summary>
/// Exception for rate limiting
/// </summary>
public class RateLimitExceededException : HealthHubException
{
    public RateLimitExceededException(string message, string? details = null)
        : base(message, "RATE_LIMIT_EXCEEDED", details) { }
}