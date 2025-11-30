using HealthHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Integration.Tests;

// Integration tests connecting to running Docker container
public class GraphQLIntegrationTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly string _baseUrl = "http://localhost:5001";
    private readonly string _testConnectionString = "Host=localhost;Port=5433;Database=healthhub_test;Username=test_user;Password=test_password";

    public GraphQLIntegrationTests()
    {
        _client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        // Use the demo auth endpoint
        var response = await _client.PostAsJsonAsync("/auth/token", new { username = "testuser", password = "testpass" });
        response.EnsureSuccessStatusCode();
        
        var tokenResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return tokenResponse?.Token ?? string.Empty;
    }

    private async Task<HttpResponseMessage> SendGraphQLQueryAsync(string query, string? token = null)
    {
        var request = new GraphQLRequest
        {
            query = query
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/graphql")
        {
            Content = content
        };

        if (!string.IsNullOrEmpty(token))
        {
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await _client.SendAsync(requestMessage);
    }

    [Fact]
    public async Task GetPatients_WithValidToken_ShouldReturnPatients()
    {
        // Arrange
        var token = await GetAuthTokenAsync();

        var query = @"
            query {
                patients {
                    nodes {
                        id
                    }
                }
            }
        ";

        // Act
        var response = await SendGraphQLQueryAsync(query, token);
        // GraphQL always returns 200, even with errors - don't call EnsureSuccessStatusCode()

        var result = await response.Content.ReadFromJsonAsync<GraphQLResponse<PatientsResponse>>();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Patients);
        Assert.NotNull(result.Data.Patients.Nodes);
        Assert.NotEmpty(result.Data.Patients.Nodes);
    }

    [Fact]
    public async Task GetPatients_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var query = @"
            query {
                patients {
                    nodes {
                        id
                    }
                }
            }
        ";

        // Act
        var response = await SendGraphQLQueryAsync(query);
        // GraphQL always returns 200, even with errors - don't call EnsureSuccessStatusCode()

        var content = await response.Content.ReadAsStringAsync();
        var result = await response.Content.ReadFromJsonAsync<GraphQLResponse<PatientsResponse>>();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("authorized", result.Errors[0]?.Message ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreatePatient_WithValidData_ShouldCreatePatient()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        
        var mutation = @"
            mutation CreatePatient($input: CreatePatientCommandInput!) {
                createPatient(command: $input) {
                    id
                    firstName
                    lastName
                }
            }
        ";


        var variables = new
        {
            input = new
            {
                firstName = "Integration",
                lastName = "Test",
                dateOfBirth = "1990-01-01"
            }
        };

        var request = new GraphQLRequest
        {
            query = mutation,
            variables = variables
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/graphql")
        {
            Content = content
        };
        requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(requestMessage);
        // GraphQL always returns 200, even with errors - don't call EnsureSuccessStatusCode()

        var result = await response.Content.ReadFromJsonAsync<GraphQLResponse<CreatePatientResponse>>();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.CreatePatient);
        Assert.Equal("Integration", result.Data.CreatePatient.FirstName);
        Assert.Equal("Test", result.Data.CreatePatient.LastName);
        Assert.NotEqual(Guid.Empty, result.Data.CreatePatient.Id);
    }

    [Fact]
    public async Task GetPatientById_WithValidId_ShouldReturnPatient()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        
        // Use an existing patient ID from the test database
        var existingPatientId = "01fafaa2-546c-4f64-b84d-516ddee9d276";

        var query = @"
            query GetPatient($id: UUID!) {
                patient(id: $id) {
                    id
                }
            }
        ";

        var variables = new { id = existingPatientId };

        var request = new GraphQLRequest
        {
            query = query,
            variables = variables
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/graphql")
        {
            Content = content
        };
        requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(requestMessage);
        // GraphQL always returns 200, even with errors - don't call EnsureSuccessStatusCode()

        var result = await response.Content.ReadFromJsonAsync<GraphQLResponse<GetPatientResponse>>();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Patient);
        Assert.Equal(existingPatientId, result.Data.Patient.Id.ToString());
    }

    // Helper classes for GraphQL responses
    private record GraphQLRequest
    {
        public string query { get; set; } = string.Empty;
        public object? variables { get; set; }
    }

    private record GraphQLResponse<T>
    {
        public T? Data { get; set; }
        public GraphQLError[]? Errors { get; set; }
    }

    private record GraphQLError
    {
        public string Message { get; set; } = string.Empty;
        public GraphQLLocation[]? Locations { get; set; }
        public string[]? Path { get; set; }
        public GraphQLErrorExtensions? Extensions { get; set; }
    }

    private record GraphQLLocation
    {
        public int Line { get; set; }
        public int Column { get; set; }
    }

    private record GraphQLErrorExtensions
    {
        public string? Code { get; set; }
    }

    private record AuthResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    private record PatientsResponse
    {
        public PatientsConnection? Patients { get; set; }
    }

    private record PatientsConnection
    {
        public PatientDto[] Nodes { get; set; } = Array.Empty<PatientDto>();
    }

    private record CreatePatientResponse
    {
        public PatientDto CreatePatient { get; set; } = new();
    }

    private record GetPatientResponse
    {
        public PatientDetailDto? Patient { get; set; }
    }

    private record PatientDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public int Age { get; set; }
    }

    private record PatientDetailDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public int Age { get; set; }
        public DiagnosticResultDto[] DiagnosticResults { get; set; } = Array.Empty<DiagnosticResultDto>();
    }

    private record DiagnosticResultDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime TimestampUtc { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}