using System.Net.Http;
using System.Text;
using HealthHub.Maui.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HealthHub.Maui.Services;

public class GraphQLClientService
{
    protected readonly HttpClient _httpClient;
    protected readonly IAuthService _authService;

    public GraphQLClientService(HttpClient httpClient, IAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    protected async Task<T> ExecuteQueryAsync<T>(string query, object? variables = null)
    {
        var request = new
        {
            query,
            variables
        };

        // Add authorization header if authenticated
        if (_authService.IsAuthenticated && !string.IsNullOrEmpty(_authService.CurrentToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.CurrentToken);
        }

        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"GraphQL Error: {response.StatusCode} - {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<GraphQLResponse<T>>(responseContent);

        if (responseObject == null)
        {
            throw new Exception("GraphQL response is null");
        }

        if (responseObject.Errors != null && responseObject.Errors.Any())
        {
            throw new Exception($"GraphQL Error: {string.Join(", ", responseObject.Errors.Select(e => e.Message))}");
        }

        return responseObject.Data;
    }

    protected async Task<T> ExecuteMutationAsync<T>(string mutation, object? variables = null)
    {
        return await ExecuteQueryAsync<T>(mutation, variables);
    }

    // Helper class for GraphQL response
    private class GraphQLResponse<T>
    {
        public T Data { get; set; }
        public List<GraphQLError> Errors { get; set; }
    }

    private class GraphQLError
    {
        public string? Message { get; set; }
    }
}