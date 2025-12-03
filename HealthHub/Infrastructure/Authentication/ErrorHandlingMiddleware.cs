using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using HealthHub.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HealthHub.Infrastructure.Authentication;

/// <summary>
/// Middleware for handling exceptions and converting them to standardized error responses
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorResponse) = CreateErrorResponse(context, exception);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }

    private (HttpStatusCode statusCode, object errorResponse) CreateErrorResponse(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var errorCode = "INTERNAL_SERVER_ERROR";
        var message = "An unexpected error occurred";
        var details = exception.Message;

        switch (exception)
        {
            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                errorCode = "NOT_FOUND";
                message = notFoundEx.Message;
                details = notFoundEx.Details ?? details;
                break;

            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "VALIDATION_ERROR";
                message = validationEx.Message;
                details = validationEx.Details ?? details;
                break;

            case RateLimitExceededException rateLimitEx:
                statusCode = HttpStatusCode.TooManyRequests;
                errorCode = "RATE_LIMIT_EXCEEDED";
                message = rateLimitEx.Message;
                details = rateLimitEx.Details ?? details;
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                errorCode = "UNAUTHORIZED";
                message = "Unauthorized access";
                break;

            case ArgumentException:
            case InvalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "BAD_REQUEST";
                message = exception.Message;
                break;

            default:
                // Log unexpected errors
                _logger.LogError(exception, "Unexpected error occurred: {Message}", exception.Message);
                message = "An unexpected error occurred. Please try again later.";
                details = "Internal server error";
                break;
        }

        var errorResponse = new
        {
            error = new
            {
                code = errorCode,
                message = message,
                details = details,
                timestamp = DateTime.UtcNow,
                path = context?.Request?.Path.ToString()
            }
        };

        return (statusCode, errorResponse);
    }
}

/// <summary>
/// Extension method for adding the error handling middleware
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}