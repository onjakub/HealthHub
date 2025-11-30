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
        CleanDatabaseAsync().Wait();
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    private async Task CleanDatabaseAsync()
    {
        try
        {
            var options = new DbContextOptionsBuilder<HealthHubDbContext>()
                .UseNpgsql(_testConnectionString)
                .Options;

            using var context = new HealthHubDbContext(options);
            
            // Delete all diagnostic results first (due to foreign key constraints)
            var diagnosticResults = await context.DiagnosticResults.ToListAsync();
            if (diagnosticResults.Any())
            {
                context.DiagnosticResults.RemoveRange(diagnosticResults);
                await context.SaveChangesAsync();
            }

            // Delete all patients
            var patients = await context.Patients.ToListAsync();
            if (patients.Any())
            {
                context.Patients.RemoveRange(patients);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail the test - database might not be available yet
            Console.WriteLine($"Warning: Could not clean database: {ex.Message}");
        }
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

        // First create a patient to ensure there's data to retrieve
        var createPatientMutation = @"
            mutation CreatePatient($input: CreatePatientCommandInput!) {
                createPatient(command: $input) {
                    id
                    firstName
                    lastName
                }
            }
        ";

        var createPatientVariables = new
        {
            input = new
            {
                firstName = "GetPatients",
                lastName = "Test",
                dateOfBirth = "1990-01-01"
            }
        };

        var createPatientRequest = new GraphQLRequest
        {
            query = createPatientMutation,
            variables = createPatientVariables
        };

        var createPatientJson = JsonSerializer.Serialize(createPatientRequest);
        var createPatientContent = new StringContent(createPatientJson, Encoding.UTF8, "application/json");
        
        var createPatientRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/graphql")
        {
            Content = createPatientContent
        };
        createPatientRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createPatientResponse = await _client.SendAsync(createPatientRequestMessage);
        var createPatientResult = await createPatientResponse.Content.ReadFromJsonAsync<GraphQLResponse<CreatePatientResponse>>();
        Assert.NotNull(createPatientResult?.Data?.createPatient);

        // Now get the patients
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
        Assert.NotNull(result.Data.createPatient);
        Assert.Equal("Integration", result.Data.createPatient.FirstName);
        Assert.Equal("Test", result.Data.createPatient.LastName);
        Assert.NotEqual(Guid.Empty, result.Data.createPatient.Id);
    }

    [Fact]
    public async Task CreatePatient_WithValidData_ShouldCreatePatientAndReturnDetails()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        
        var mutation = @"
            mutation CreatePatient($input: CreatePatientCommandInput!) {
                createPatient(command: $input) {
                    id
                    firstName
                    lastName
                    dateOfBirth
                    age
                }
            }
        ";

        var variables = new
        {
            input = new
            {
                firstName = "Diagnosis",
                lastName = "Test",
                dateOfBirth = "1985-05-15"
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
        Assert.NotNull(result.Data.createPatient);
        Assert.Equal("Diagnosis", result.Data.createPatient.FirstName);
        Assert.Equal("Test", result.Data.createPatient.LastName);
        Assert.NotEqual(Guid.Empty, result.Data.createPatient.Id);
        Assert.True(result.Data.createPatient.Age > 0); // Age should be calculated
    }

    [Fact]
    public async Task GetPatientById_WithValidId_ShouldReturnPatient()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        
        // Create a new patient for this test to avoid dependency on existing data
        var createPatientMutation = @"
            mutation CreatePatient($input: CreatePatientCommandInput!) {
                createPatient(command: $input) {
                    id
                    firstName
                    lastName
                }
            }
        ";

        var createPatientVariables = new
        {
            input = new
            {
                firstName = "GetById",
                lastName = "Test",
                dateOfBirth = "1995-03-20"
            }
        };

        var createPatientRequest = new GraphQLRequest
        {
            query = createPatientMutation,
            variables = createPatientVariables
        };

        var createPatientJson = JsonSerializer.Serialize(createPatientRequest);
        var createPatientContent = new StringContent(createPatientJson, Encoding.UTF8, "application/json");
        
        var createPatientRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/graphql")
        {
            Content = createPatientContent
        };
        createPatientRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createPatientResponse = await _client.SendAsync(createPatientRequestMessage);
        var createPatientResult = await createPatientResponse.Content.ReadFromJsonAsync<GraphQLResponse<CreatePatientResponse>>();
        var patientId = createPatientResult?.Data?.createPatient?.Id ?? Guid.Empty;

        var query = @"
            query GetPatient($id: UUID!) {
                patient(id: $id) {
                    id
                }
            }
        ";

        var variables = new { id = patientId };

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
        Assert.Equal(patientId, result.Data.Patient.Id);
    }

    [Fact]
    public async Task AddDiagnosticResult_WithValidData_ShouldCreateDiagnosis()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        
        // First create a patient
        var createPatientMutation = @"
            mutation CreatePatient($input: CreatePatientCommandInput!) {
                createPatient(command: $input) {
                    id
                    firstName
                    lastName
                }
            }
        ";

        var createPatientVariables = new
        {
            input = new
            {
                firstName = "Diagnosis",
                lastName = "Patient",
                dateOfBirth = "1980-06-15"
            }
        };

        var createPatientRequest = new GraphQLRequest
        {
            query = createPatientMutation,
            variables = createPatientVariables
        };

        var createPatientJson = JsonSerializer.Serialize(createPatientRequest);
        var createPatientContent = new StringContent(createPatientJson, Encoding.UTF8, "application/json");
        
        var createPatientRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/graphql")
        {
            Content = createPatientContent
        };
        createPatientRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createPatientResponse = await _client.SendAsync(createPatientRequestMessage);
        var createPatientResult = await createPatientResponse.Content.ReadFromJsonAsync<GraphQLResponse<CreatePatientResponse>>();
        var patientId = createPatientResult?.Data?.createPatient?.Id ?? Guid.Empty;

        // Now add a diagnostic result
        var mutation = @"
            mutation AddDiagnosticResult($input: AddDiagnosticResultCommandInput!) {
                addDiagnosticResult(command: $input) {
                    id
                    patientId
                    diagnosis
                    notes
                    timestampUtc
                }
            }
        ";

        var variables = new
        {
            input = new
            {
                patientId = patientId,
                diagnosis = "Hypertension",
                notes = "Stage 1 hypertension, lifestyle modifications recommended"
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

        var result = await response.Content.ReadFromJsonAsync<GraphQLResponse<CreateDiagnosticResultResponse>>();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.addDiagnosticResult);
        Assert.Equal(patientId, result.Data.addDiagnosticResult.PatientId);
        Assert.Equal("Hypertension", result.Data.addDiagnosticResult.Diagnosis);
        Assert.Equal("Stage 1 hypertension, lifestyle modifications recommended", result.Data.addDiagnosticResult.Notes);
        Assert.NotEqual(Guid.Empty, result.Data.addDiagnosticResult.Id);
        Assert.True(result.Data.addDiagnosticResult.TimestampUtc > DateTime.UtcNow.AddMinutes(-5)); // Should be recent
    }

    [Fact]
    public async Task GetPatientDiagnosticResults_WithValidPatientId_ShouldReturnDiagnoses()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        
        // First create a patient
        var createPatientMutation = @"
            mutation CreatePatient($input: CreatePatientCommandInput!) {
                createPatient(command: $input) {
                    id
                    firstName
                    lastName
                }
            }
        ";

        var createPatientVariables = new
        {
            input = new
            {
                firstName = "Diagnostic",
                lastName = "Results",
                dateOfBirth = "1975-12-10"
            }
        };

        var createPatientRequest = new GraphQLRequest
        {
            query = createPatientMutation,
            variables = createPatientVariables
        };

        var createPatientJson = JsonSerializer.Serialize(createPatientRequest);
        var createPatientContent = new StringContent(createPatientJson, Encoding.UTF8, "application/json");
        
        var createPatientRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/graphql")
        {
            Content = createPatientContent
        };
        createPatientRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createPatientResponse = await _client.SendAsync(createPatientRequestMessage);
        var createPatientResult = await createPatientResponse.Content.ReadFromJsonAsync<GraphQLResponse<CreatePatientResponse>>();
        var patientId = createPatientResult?.Data?.createPatient?.Id ?? Guid.Empty;

        // Add two diagnostic results
        var addDiagnosisMutation = @"
            mutation AddDiagnosticResult($input: AddDiagnosticResultCommandInput!) {
                addDiagnosticResult(command: $input) {
                    id
                }
            }
        ";

        // Add first diagnosis
        var firstDiagnosisVariables = new
        {
            input = new
            {
                patientId = patientId,
                diagnosis = "Diabetes mellitus type 2",
                notes = "Controlled with medication"
            }
        };

        var firstDiagnosisRequest = new GraphQLRequest
        {
            query = addDiagnosisMutation,
            variables = firstDiagnosisVariables
        };

        var firstDiagnosisJson = JsonSerializer.Serialize(firstDiagnosisRequest);
        var firstDiagnosisContent = new StringContent(firstDiagnosisJson, Encoding.UTF8, "application/json");
        
        var firstDiagnosisRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/graphql")
        {
            Content = firstDiagnosisContent
        };
        firstDiagnosisRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await _client.SendAsync(firstDiagnosisRequestMessage);

        // Add second diagnosis
        var secondDiagnosisVariables = new
        {
            input = new
            {
                patientId = patientId,
                diagnosis = "Hyperlipidemia",
                notes = "Statin therapy initiated"
            }
        };

        var secondDiagnosisRequest = new GraphQLRequest
        {
            query = addDiagnosisMutation,
            variables = secondDiagnosisVariables
        };

        var secondDiagnosisJson = JsonSerializer.Serialize(secondDiagnosisRequest);
        var secondDiagnosisContent = new StringContent(secondDiagnosisJson, Encoding.UTF8, "application/json");
        
        var secondDiagnosisRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/graphql")
        {
            Content = secondDiagnosisContent
        };
        secondDiagnosisRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        await _client.SendAsync(secondDiagnosisRequestMessage);

        // Now get the diagnostic results
        var query = @"
            query GetPatientDiagnosticResults($patientId: UUID!) {
                patientDiagnosticResults(patientId: $patientId) {
                    id
                    diagnosis
                    notes
                    timestampUtc
                }
            }
        ";

        var variables = new { patientId = patientId };

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

        var result = await response.Content.ReadFromJsonAsync<GraphQLResponse<GetPatientDiagnosticResultsResponse>>();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.patientDiagnosticResults);
        Assert.Equal(2, result.Data.patientDiagnosticResults.Length);
        
        var diagnoses = result.Data.patientDiagnosticResults.Select(d => d.Diagnosis).ToList();
        Assert.Contains("Diabetes mellitus type 2", diagnoses);
        Assert.Contains("Hyperlipidemia", diagnoses);
    }

    [Fact]
    public async Task UpdateDiagnosticResult_WithValidData_ShouldUpdateDiagnosis()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        
        // First create a patient
        var createPatientMutation = @"
            mutation CreatePatient($input: CreatePatientCommandInput!) {
                createPatient(command: $input) {
                    id
                    firstName
                    lastName
                }
            }
        ";

        var createPatientVariables = new
        {
            input = new
            {
                firstName = "Update",
                lastName = "Diagnosis",
                dateOfBirth = "1988-03-25"
            }
        };

        var createPatientRequest = new GraphQLRequest
        {
            query = createPatientMutation,
            variables = createPatientVariables
        };

        var createPatientJson = JsonSerializer.Serialize(createPatientRequest);
        var createPatientContent = new StringContent(createPatientJson, Encoding.UTF8, "application/json");
        
        var createPatientRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/graphql")
        {
            Content = createPatientContent
        };
        createPatientRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createPatientResponse = await _client.SendAsync(createPatientRequestMessage);
        var createPatientResult = await createPatientResponse.Content.ReadFromJsonAsync<GraphQLResponse<CreatePatientResponse>>();
        var patientId = createPatientResult?.Data?.createPatient?.Id ?? Guid.Empty;

        // Add a diagnostic result to update
        var addDiagnosisMutation = @"
            mutation AddDiagnosticResult($input: AddDiagnosticResultCommandInput!) {
                addDiagnosticResult(command: $input) {
                    id
                    diagnosis
                    notes
                }
            }
        ";

        var addDiagnosisVariables = new
        {
            input = new
            {
                patientId = patientId,
                diagnosis = "Initial diagnosis",
                notes = "Initial notes"
            }
        };

        var addDiagnosisRequest = new GraphQLRequest
        {
            query = addDiagnosisMutation,
            variables = addDiagnosisVariables
        };

        var addDiagnosisJson = JsonSerializer.Serialize(addDiagnosisRequest);
        var addDiagnosisContent = new StringContent(addDiagnosisJson, Encoding.UTF8, "application/json");
        
        var addDiagnosisRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/graphql")
        {
            Content = addDiagnosisContent
        };
        addDiagnosisRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var addDiagnosisResponse = await _client.SendAsync(addDiagnosisRequestMessage);
        var addDiagnosisResult = await addDiagnosisResponse.Content.ReadFromJsonAsync<GraphQLResponse<CreateDiagnosticResultResponse>>();
        var diagnosticResultId = addDiagnosisResult?.Data?.addDiagnosticResult?.Id ?? Guid.Empty;

        // Now update the diagnostic result
        var updateMutation = @"
            mutation UpdateDiagnosticResult($input: UpdateDiagnosticResultCommandInput!) {
                updateDiagnosticResult(command: $input) {
                    id
                    diagnosis
                    notes
                }
            }
        ";

        var updateVariables = new
        {
            input = new
            {
                diagnosticResultId = diagnosticResultId,
                notes = "Updated notes with more details"
            }
        };

        var updateRequest = new GraphQLRequest
        {
            query = updateMutation,
            variables = updateVariables
        };

        var updateJson = JsonSerializer.Serialize(updateRequest);
        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");
        
        var updateRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/graphql")
        {
            Content = updateContent
        };
        updateRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(updateRequestMessage);

        var result = await response.Content.ReadFromJsonAsync<GraphQLResponse<UpdateDiagnosticResultResponse>>();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.updateDiagnosticResult);
        Assert.Equal(diagnosticResultId, result.Data.updateDiagnosticResult.Id);
        Assert.Equal("Initial diagnosis", result.Data.updateDiagnosticResult.Diagnosis); // Diagnosis should remain unchanged
        Assert.Equal("Updated notes with more details", result.Data.updateDiagnosticResult.Notes);
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
        public PatientDto createPatient { get; set; } = new();
    }

    private record GetPatientResponse
    {
        public PatientDetailDto? Patient { get; set; }
    }

    private record CreateDiagnosticResultResponse
    {
        public DiagnosticResultDto addDiagnosticResult { get; set; } = new();
    }

    private record UpdateDiagnosticResultResponse
    {
        public DiagnosticResultDto updateDiagnosticResult { get; set; } = new();
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

    private record GetPatientDiagnosticResultsResponse
    {
        public DiagnosticResultDto[] patientDiagnosticResults { get; set; } = Array.Empty<DiagnosticResultDto>();
    }
}