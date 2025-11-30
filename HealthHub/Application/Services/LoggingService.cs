using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HealthHub.Application.Services;

public interface ILoggingService
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(string message, params object[] args);
    void LogError(Exception exception, string message, params object[] args);
    void LogAudit(string action, string entityType, string entityId, string userId, string details);
    IDisposable StartActivity(string operationName);
}

public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;

    public LoggingService(ILogger<LoggingService> logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }

    public void LogError(string message, params object[] args)
    {
        _logger.LogError(message, args);
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        _logger.LogError(exception, message, args);
    }

    public void LogAudit(string action, string entityType, string entityId, string userId, string details)
    {
        _logger.LogInformation("AUDIT: {Action} on {EntityType} {EntityId} by {UserId} - {Details}", 
            action, entityType, entityId, userId, details);
    }

    public IDisposable StartActivity(string operationName)
    {
        var activity = new Activity(operationName);
        activity.Start();
        return new ActivityScope(activity);
    }

    private class ActivityScope : IDisposable
    {
        private readonly Activity _activity;

        public ActivityScope(Activity activity)
        {
            _activity = activity;
        }

        public void Dispose()
        {
            _activity.Stop();
        }
    }
}