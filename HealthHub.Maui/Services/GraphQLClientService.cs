using GraphQL;
using GraphQL.Client;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using HealthHub.Maui.Models;
using Newtonsoft.Json.Linq;

namespace HealthHub.Maui.Services;

public class GraphQLClientService
{
    protected readonly IGraphQLClient _client;
    protected readonly IAuthService _authService;

    public GraphQLClientService(IGraphQLClient client, IAuthService authService)
    {
        _client = client;
        _authService = authService;
    }

    protected async Task<T> ExecuteQueryAsync<T>(string query, object? variables = null)
    {
        var request = new GraphQLRequest
        {
            Query = query,
            Variables = variables
        };

        // Add authorization header if authenticated
        if (_authService.IsAuthenticated && !string.IsNullOrEmpty(_authService.CurrentToken))
        {
            _client.HttpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.CurrentToken);
        }

        var response = await _client.SendQueryAsync<T>(request);
        
        if (response.Errors != null && response.Errors.Any())
        {
            throw new Exception($"GraphQL Error: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        return response.Data;
    }

    protected async Task<T> ExecuteMutationAsync<T>(string mutation, object? variables = null)
    {
        var request = new GraphQLRequest
        {
            Query = mutation,
            Variables = variables
        };

        // Add authorization header if authenticated
        if (_authService.IsAuthenticated && !string.IsNullOrEmpty(_authService.CurrentToken))
        {
            _client.HttpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.CurrentToken);
        }

        var response = await _client.SendMutationAsync<T>(request);
        
        if (response.Errors != null && response.Errors.Any())
        {
            throw new Exception($"GraphQL Error: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        return response.Data;
    }
}