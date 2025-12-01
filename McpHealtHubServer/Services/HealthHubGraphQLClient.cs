using System.Text;
using System.Text.Json;
using McpHealtHubServer.Configuration;
using McpHealtHubServer.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace McpHealtHubServer.Services;

public interface IHealthHubGraphQLClient
{
    Task<JsonDocument> SendQueryAsync(object query, CancellationToken cancellationToken = default);
    Task<JsonDocument> SendQueryAsync(string query, object? variables = null, CancellationToken cancellationToken = default);
}

public class HealthHubGraphQLClient : IHealthHubGraphQLClient
{
    private readonly HttpClient _httpClient;
    private readonly IJwtTokenService _tokenService;
    private readonly ILogger<HealthHubGraphQLClient> _logger;
    private readonly HealthHubConfiguration _configuration;

    public HealthHubGraphQLClient(
        HttpClient httpClient,
        IJwtTokenService tokenService,
        IOptions<HealthHubConfiguration> configuration,
        ILogger<HealthHubGraphQLClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
        
        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_configuration.ApiUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_configuration.HttpTimeoutSeconds);
        
        _logger.LogInformation("HTTP client configured with base address: {BaseAddress}, timeout: {Timeout}s", 
            _httpClient.BaseAddress, _configuration.HttpTimeoutSeconds);
    }

    public async Task<JsonDocument> SendQueryAsync(object query, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Sending GraphQL query: {@Query}", query);
        
        try
        {
            var json = JsonSerializer.Serialize(query);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "graphql")
            {
                Content = content
            };

            await AddAuthenticationHeaderAsync(requestMessage, cancellationToken);
            
            _logger.LogDebug("Sending HTTP request to: {Url}", requestMessage.RequestUri);
            
            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("HTTP request failed with status code: {StatusCode}, reason: {ReasonPhrase}", 
                    response.StatusCode, response.ReasonPhrase);
                
                throw new HealthHubApiException(
                    $"HTTP request failed with status code: {response.StatusCode}", 
                    (int)response.StatusCode);
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Received response: {ResponseContent}", responseContent);
            
            var jsonDocument = JsonDocument.Parse(responseContent);
            
            await ValidateGraphQLResponseAsync(jsonDocument, cancellationToken);
            
            return jsonDocument;
        }
        catch (HealthHubException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error sending GraphQL query");
            throw new HealthHubException("Error sending GraphQL query", ex);
        }
    }

    public async Task<JsonDocument> SendQueryAsync(string query, object? variables = null, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            query,
            variables
        };
        
        return await SendQueryAsync(request, cancellationToken);
    }

    private async Task AddAuthenticationHeaderAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
    {
        if (!_tokenService.IsTokenValid())
        {
            _logger.LogWarning("JWT token is invalid or expired");
            throw new HealthHubConfigurationException("JWT token is invalid or expired");
        }

        if (_tokenService.IsTokenExpiringSoon())
        {
            _logger.LogWarning("JWT token is expiring soon. Consider refreshing the token.");
        }

        var token = _tokenService.GetToken();
        requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        _logger.LogDebug("Added authentication header to request");
    }

    private async Task ValidateGraphQLResponseAsync(JsonDocument response, CancellationToken cancellationToken)
    {
        // Check for GraphQL errors
        if (response.RootElement.TryGetProperty("errors", out var errors) && 
            errors.ValueKind == JsonValueKind.Array && 
            errors.GetArrayLength() > 0)
        {
            var errorMessages = new List<string>();
            foreach (var error in errors.EnumerateArray())
            {
                if (error.TryGetProperty("message", out var message))
                {
                    var errorMessage = message.GetString() ?? "Unknown error";
                    errorMessages.Add(errorMessage);
                    _logger.LogError("GraphQL error: {ErrorMessage}", errorMessage);
                }
            }
            
            throw new HealthHubGraphQLException(errorMessages.ToArray());
        }

        // Check for data property
        if (!response.RootElement.TryGetProperty("data", out _))
        {
            _logger.LogError("GraphQL response missing 'data' property");
            throw new HealthHubGraphQLException("Invalid GraphQL response: missing 'data' property");
        }
        
        _logger.LogDebug("GraphQL response validated successfully");
    }
}