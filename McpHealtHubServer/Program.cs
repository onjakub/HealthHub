using System.ComponentModel.DataAnnotations;
using McpHealtHubServer.Configuration;
using McpHealtHubServer.Services;
using McpHealtHubServer.Tools;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Add configuration
builder.Services.Configure<HealthHubConfiguration>(options =>
{
    options.ApiUrl = Environment.GetEnvironmentVariable("HEALTHHUB_API_URL") ?? "http://localhost:5000";
    options.JwtToken = Environment.GetEnvironmentVariable("HEALTHHUB_JWT_TOKEN") ?? throw new InvalidOperationException("HEALTHHUB_JWT_TOKEN environment variable is required");
    options.HttpTimeoutSeconds = int.TryParse(Environment.GetEnvironmentVariable("HEALTHHUB_HTTP_TIMEOUT_SECONDS"), out var timeout) ? timeout : 30;
    options.CacheExpirationMinutes = int.TryParse(Environment.GetEnvironmentVariable("HEALTHHUB_CACHE_EXPIRATION_MINUTES"), out var cacheExpiration) ? cacheExpiration : 5;
    
    // Validate configuration
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(options);
    if (!Validator.TryValidateObject(options, validationContext, validationResults, true))
    {
        throw new InvalidOperationException($"Configuration validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}");
    }
});

// Add memory cache
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Limit cache to 1024 entries
});

// Add HTTP client with typed client configuration
builder.Services.AddHttpClient<IHealthHubGraphQLClient, HealthHubGraphQLClient>(client =>
{
    var apiUrl = Environment.GetEnvironmentVariable("HEALTHHUB_API_URL") ?? "http://localhost:5000";
    client.BaseAddress = new Uri(apiUrl);
    client.Timeout = TimeSpan.FromSeconds(int.TryParse(Environment.GetEnvironmentVariable("HEALTHHUB_HTTP_TIMEOUT_SECONDS"), out var timeout) ? timeout : 30);
});

// Add services
builder.Services.AddSingleton<IJwtTokenService>(provider =>
{
    var token = Environment.GetEnvironmentVariable("HEALTHHUB_JWT_TOKEN") ?? throw new InvalidOperationException("HEALTHHUB_JWT_TOKEN environment variable is required");
    var logger = provider.GetRequiredService<ILogger<JwtTokenService>>();
    return new JwtTokenService(token, logger);
});

builder.Services.AddScoped<IHealthHubGraphQLClient, HealthHubGraphQLClient>();
builder.Services.AddScoped<IResponseFormatter, ResponseFormatter>();
builder.Services.AddScoped<IPatientService, PatientService>();

// Add the MCP services: the transport to use (stdio) and the tools to register.
builder.Services.AddMcpServer().WithStdioServerTransport().WithTools(new[] { typeof(RandomNumberTools), typeof(PatientTools) });

await builder.Build().RunAsync();