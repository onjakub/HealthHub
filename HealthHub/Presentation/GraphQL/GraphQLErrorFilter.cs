using System;
using System.Collections.Generic;
using HealthHub.Domain.Exceptions;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.Logging;

namespace HealthHub.Presentation.GraphQL;

/// <summary>
/// Error filter for GraphQL to convert exceptions to standardized error responses
/// </summary>
public class GraphQLErrorFilter : IErrorFilter
{
    private readonly ILogger<GraphQLErrorFilter> _logger;

    public GraphQLErrorFilter(ILogger<GraphQLErrorFilter> logger)
    {
        _logger = logger;
    }

    public IError OnError(IError error)
    {
        if (error.Exception is HealthHubException healthHubEx)
        {
            return CreateHealthHubError(error, healthHubEx);
        }

        return error.Exception switch
        {
            UnauthorizedAccessException => CreateUnauthorizedError(error),
            ArgumentException or ArgumentNullException or InvalidOperationException => CreateBadRequestError(error),
            _ => HandleUnexpectedError(error)
        };
    }

    private IError CreateHealthHubError(IError error, HealthHubException exception)
    {
        var errorBuilder = ErrorBuilder.New()
            .SetMessage(exception.Message)
            .SetCode(exception.ErrorCode)
            .SetPath(error.Path)
            .SetExtension("timestamp", DateTime.UtcNow)
            .SetExtension("details", exception.Details ?? exception.Message)
            .SetExtension("code", exception.ErrorCode);

        // Map specific exception types to appropriate error codes
        errorBuilder = exception switch
        {
            NotFoundException => errorBuilder.SetCode("NOT_FOUND"),
            ValidationException => errorBuilder.SetCode("VALIDATION_ERROR"),
            UnauthorizedException => errorBuilder.SetCode("UNAUTHORIZED"),
            RateLimitExceededException => errorBuilder.SetCode("RATE_LIMIT_EXCEEDED"),
            _ => errorBuilder
        };

        return errorBuilder.Build();
    }

    private IError CreateUnauthorizedError(IError error)
    {
        return ErrorBuilder.New()
            .SetMessage("Unauthorized access")
            .SetCode("UNAUTHORIZED")
            .SetPath(error.Path)
            .SetExtension("timestamp", DateTime.UtcNow)
            .SetExtension("details", error.Exception?.Message ?? "No access to this resource")
            .SetExtension("code", "UNAUTHORIZED")
            .Build();
    }

    private IError CreateBadRequestError(IError error)
    {
        return ErrorBuilder.New()
            .SetMessage("Invalid request")
            .SetCode("BAD_REQUEST")
            .SetPath(error.Path)
            .SetExtension("timestamp", DateTime.UtcNow)
            .SetExtension("details", error.Exception?.Message ?? "Invalid request parameters")
            .SetExtension("code", "BAD_REQUEST")
            .Build();
    }

    private IError HandleUnexpectedError(IError error)
    {
        _logger.LogError(error.Exception, "Unexpected GraphQL error occurred: {Message}", error.Message);

        return ErrorBuilder.New()
            .SetMessage("An unexpected error occurred. Please try again later.")
            .SetCode("INTERNAL_SERVER_ERROR")
            .SetPath(error.Path)
            .SetExtension("timestamp", DateTime.UtcNow)
            .SetExtension("details", "Internal server error")
            .SetExtension("code", "INTERNAL_SERVER_ERROR")
            .Build();
    }
}