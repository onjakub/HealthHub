using System.ComponentModel;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace McpHealtHubServer.Tools;

/// <summary>
/// MCP tools for accessing patient information from HealthHub system.
/// </summary>
internal class PatientTools
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private readonly string _jwtToken;

    public PatientTools(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _apiBaseUrl = Environment.GetEnvironmentVariable("HEALTHHUB_API_URL") ?? "http://localhost:5000";
        _jwtToken = Environment.GetEnvironmentVariable("HEALTHHUB_JWT_TOKEN") ?? GetDefaultToken();
    }

    [McpServerTool]
    [Description("Gets a list of patients with optional search and pagination.")]
    public async Task<string> GetPatients(
        [Description("Search term to filter patients by name or diagnosis")] string? searchTerm = null,
        [Description("Page number for pagination (starts at 1)")] int? page = null,
        [Description("Number of patients per page")] int? pageSize = null)
    {
        try
        {
            var query = new
            {
                query = @"
                    query GetPatients($searchTerm: String, $page: Int, $pageSize: Int) {
                        patients(searchTerm: $searchTerm, page: $page, pageSize: $pageSize) {
                            nodes {
                                id
                                firstName
                                lastName
                                fullName
                                dateOfBirth
                                age
                                lastDiagnosis
                                createdAt
                                updatedAt
                            }
                        }
                    }",
                variables = new { searchTerm, page, pageSize }
            };

            var response = await SendGraphQLRequest(query);
            return FormatPatientListResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error retrieving patients: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Gets detailed information about a specific patient by ID.")]
    public async Task<string> GetPatientById(
        [Description("The unique identifier of the patient")] Guid patientId)
    {
        try
        {
            var query = new
            {
                query = @"
                    query GetPatient($id: UUID!) {
                        patient(id: $id) {
                            id
                            firstName
                            lastName
                            fullName
                            dateOfBirth
                            age
                            lastDiagnosis
                            createdAt
                            updatedAt
                            diagnosticResults {
                                id
                                diagnosis
                                notes
                                timestampUtc
                            }
                        }
                    }",
                variables = new { id = patientId }
            };

            var response = await SendGraphQLRequest(query);
            return FormatPatientDetailResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error retrieving patient: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Gets diagnostic results for a specific patient.")]
    public async Task<string> GetPatientDiagnosticResults(
        [Description("The unique identifier of the patient")] Guid patientId,
        [Description("Maximum number of results to return")] int? limit = null)
    {
        try
        {
            var query = new
            {
                query = @"
                    query GetPatientDiagnosticResults($patientId: UUID!, $limit: Int) {
                        patientDiagnosticResults(patientId: $patientId, limit: $limit) {
                            id
                            diagnosis
                            notes
                            timestampUtc
                        }
                    }",
                variables = new { patientId, limit }
            };

            var response = await SendGraphQLRequest(query);
            return FormatDiagnosticResultsResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error retrieving diagnostic results: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Searches for patients by name or diagnosis with advanced filtering.")]
    public async Task<string> SearchPatients(
        [Description("Search term to match against patient names or diagnoses")] string searchTerm,
        [Description("Minimum age to filter by")] int? minAge = null,
        [Description("Maximum age to filter by")] int? maxAge = null,
        [Description("Filter by patients with recent diagnoses")] bool? hasRecentDiagnosis = null)
    {
        try
        {
            // First get all patients matching the search term
            var allPatients = await GetPatientsInternal(searchTerm);
            
            // Apply additional filters
            var filteredPatients = allPatients.AsEnumerable();
            
            if (minAge.HasValue)
            {
                filteredPatients = filteredPatients.Where(p => p.Age >= minAge.Value);
            }
            
            if (maxAge.HasValue)
            {
                filteredPatients = filteredPatients.Where(p => p.Age <= maxAge.Value);
            }
            
            if (hasRecentDiagnosis.HasValue)
            {
                filteredPatients = filteredPatients.Where(p => 
                    hasRecentDiagnosis.Value ? !string.IsNullOrEmpty(p.LastDiagnosis) : string.IsNullOrEmpty(p.LastDiagnosis));
            }

            return FormatPatientSearchResponse(filteredPatients.ToList(), searchTerm, minAge, maxAge, hasRecentDiagnosis);
        }
        catch (Exception ex)
        {
            return $"Error searching patients: {ex.Message}";
        }
    }

    private async Task<JsonDocument> SendGraphQLRequest(object request)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/graphql")
        {
            Content = content
        };

        if (!string.IsNullOrEmpty(_jwtToken))
        {
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);
        }

        var response = await _httpClient.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(responseContent);
    }

    private async Task<List<PatientDto>> GetPatientsInternal(string? searchTerm = null)
    {
        var query = new
        {
            query = @"
                query GetPatients($searchTerm: String) {
                    patients(searchTerm: $searchTerm) {
                        id
                        firstName
                        lastName
                        fullName
                        dateOfBirth
                        age
                        lastDiagnosis
                        createdAt
                        updatedAt
                    }
                }",
            variables = new { searchTerm }
        };

        var response = await SendGraphQLRequest(query);
        var data = response.RootElement.GetProperty("data").GetProperty("patients").GetProperty("nodes");
        var patients = new List<PatientDto>();

        foreach (var patientElement in data.EnumerateArray())
        {
            patients.Add(new PatientDto
            {
                Id = patientElement.GetProperty("id").GetGuid(),
                FirstName = patientElement.GetProperty("firstName").GetString() ?? "",
                LastName = patientElement.GetProperty("lastName").GetString() ?? "",
                FullName = patientElement.GetProperty("fullName").GetString() ?? "",
                DateOfBirth = DateOnly.Parse(patientElement.GetProperty("dateOfBirth").GetString() ?? ""),
                Age = patientElement.GetProperty("age").GetInt32(),
                LastDiagnosis = patientElement.GetProperty("lastDiagnosis").GetString(),
                CreatedAt = DateTime.Parse(patientElement.GetProperty("createdAt").GetString() ?? ""),
                UpdatedAt = patientElement.TryGetProperty("updatedAt", out var updatedAt) ? 
                    DateTime.Parse(updatedAt.GetString() ?? "") : null
            });
        }

        return patients;
    }

    private string FormatPatientListResponse(JsonDocument response)
    {
        var data = response.RootElement.GetProperty("data").GetProperty("patients");
        var patients = new List<PatientDto>();

        foreach (var patientElement in data.EnumerateArray())
        {
            patients.Add(new PatientDto
            {
                Id = patientElement.GetProperty("id").GetGuid(),
                FirstName = patientElement.GetProperty("firstName").GetString() ?? "",
                LastName = patientElement.GetProperty("lastName").GetString() ?? "",
                FullName = patientElement.GetProperty("fullName").GetString() ?? "",
                DateOfBirth = DateOnly.Parse(patientElement.GetProperty("dateOfBirth").GetString() ?? ""),
                Age = patientElement.GetProperty("age").GetInt32(),
                LastDiagnosis = patientElement.GetProperty("lastDiagnosis").GetString(),
                CreatedAt = DateTime.Parse(patientElement.GetProperty("createdAt").GetString() ?? ""),
                UpdatedAt = patientElement.TryGetProperty("updatedAt", out var updatedAt) ? 
                    DateTime.Parse(updatedAt.GetString() ?? "") : null
            });
        }

        if (patients.Count == 0)
        {
            return "No patients found.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Found {patients.Count} patients:");
        sb.AppendLine();

        foreach (var patient in patients)
        {
            sb.AppendLine($"ID: {patient.Id}");
            sb.AppendLine($"Name: {patient.FullName}");
            sb.AppendLine($"Age: {patient.Age}");
            sb.AppendLine($"Date of Birth: {patient.DateOfBirth:yyyy-MM-dd}");
            sb.AppendLine($"Last Diagnosis: {patient.LastDiagnosis ?? "None"}");
            sb.AppendLine($"Created: {patient.CreatedAt:yyyy-MM-dd HH:mm}");
            if (patient.UpdatedAt.HasValue)
            {
                sb.AppendLine($"Updated: {patient.UpdatedAt.Value:yyyy-MM-dd HH:mm}");
            }
            sb.AppendLine("---");
        }

        return sb.ToString();
    }

    private string FormatPatientDetailResponse(JsonDocument response)
    {
        var data = response.RootElement.GetProperty("data").GetProperty("patient");

        if (data.ValueKind == JsonValueKind.Null)
        {
            return "Patient not found.";
        }

        var patient = new PatientDetailDto
        {
            Id = data.GetProperty("id").GetGuid(),
            FirstName = data.GetProperty("firstName").GetString() ?? "",
            LastName = data.GetProperty("lastName").GetString() ?? "",
            FullName = data.GetProperty("fullName").GetString() ?? "",
            DateOfBirth = DateOnly.Parse(data.GetProperty("dateOfBirth").GetString() ?? ""),
            Age = data.GetProperty("age").GetInt32(),
            LastDiagnosis = data.GetProperty("lastDiagnosis").GetString(),
            CreatedAt = DateTime.Parse(data.GetProperty("createdAt").GetString() ?? ""),
            UpdatedAt = data.TryGetProperty("updatedAt", out var updatedAt) ? 
                DateTime.Parse(updatedAt.GetString() ?? "") : null
        };

        var diagnosticResults = data.GetProperty("diagnosticResults");
        foreach (var resultElement in diagnosticResults.EnumerateArray())
        {
            patient.DiagnosticResults.Add(new DiagnosticResultDto
            {
                Id = resultElement.GetProperty("id").GetGuid(),
                Diagnosis = resultElement.GetProperty("diagnosis").GetString() ?? "",
                Notes = resultElement.GetProperty("notes").GetString(),
                TimestampUtc = DateTime.Parse(resultElement.GetProperty("timestampUtc").GetString() ?? "")
            });
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Patient Details:");
        sb.AppendLine($"ID: {patient.Id}");
        sb.AppendLine($"Name: {patient.FullName}");
        sb.AppendLine($"Age: {patient.Age}");
        sb.AppendLine($"Date of Birth: {patient.DateOfBirth:yyyy-MM-dd}");
        sb.AppendLine($"Last Diagnosis: {patient.LastDiagnosis ?? "None"}");
        sb.AppendLine($"Created: {patient.CreatedAt:yyyy-MM-dd HH:mm}");
        if (patient.UpdatedAt.HasValue)
        {
            sb.AppendLine($"Updated: {patient.UpdatedAt.Value:yyyy-MM-dd HH:mm}");
        }
        sb.AppendLine();

        if (patient.DiagnosticResults.Count > 0)
        {
            sb.AppendLine($"Diagnostic Results ({patient.DiagnosticResults.Count}):");
            foreach (var result in patient.DiagnosticResults.OrderByDescending(r => r.TimestampUtc))
            {
                sb.AppendLine($"- {result.TimestampUtc:yyyy-MM-dd}: {result.Diagnosis}");
                if (!string.IsNullOrEmpty(result.Notes))
                {
                    sb.AppendLine($"  Notes: {result.Notes}");
                }
            }
        }
        else
        {
            sb.AppendLine("No diagnostic results available.");
        }

        return sb.ToString();
    }

    private string FormatDiagnosticResultsResponse(JsonDocument response)
    {
        var data = response.RootElement.GetProperty("data").GetProperty("patientDiagnosticResults");
        var results = new List<DiagnosticResultDto>();

        foreach (var resultElement in data.EnumerateArray())
        {
            results.Add(new DiagnosticResultDto
            {
                Id = resultElement.GetProperty("id").GetGuid(),
                Diagnosis = resultElement.GetProperty("diagnosis").GetString() ?? "",
                Notes = resultElement.GetProperty("notes").GetString(),
                TimestampUtc = DateTime.Parse(resultElement.GetProperty("timestampUtc").GetString() ?? "")
            });
        }

        if (results.Count == 0)
        {
            return "No diagnostic results found for this patient.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Found {results.Count} diagnostic results:");
        sb.AppendLine();

        foreach (var result in results.OrderByDescending(r => r.TimestampUtc))
        {
            sb.AppendLine($"Date: {result.TimestampUtc:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Diagnosis: {result.Diagnosis}");
            if (!string.IsNullOrEmpty(result.Notes))
            {
                sb.AppendLine($"Notes: {result.Notes}");
            }
            sb.AppendLine("---");
        }

        return sb.ToString();
    }

    private string FormatPatientSearchResponse(List<PatientDto> patients, string searchTerm, int? minAge, int? maxAge, bool? hasRecentDiagnosis)
    {
        if (patients.Count == 0)
        {
            return "No patients found matching the search criteria.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Search Results:");
        sb.AppendLine($"- Search term: {searchTerm}");
        if (minAge.HasValue) sb.AppendLine($"- Minimum age: {minAge}");
        if (maxAge.HasValue) sb.AppendLine($"- Maximum age: {maxAge}");
        if (hasRecentDiagnosis.HasValue) sb.AppendLine($"- Has recent diagnosis: {hasRecentDiagnosis}");
        sb.AppendLine($"Found {patients.Count} patients:");
        sb.AppendLine();

        foreach (var patient in patients)
        {
            sb.AppendLine($"ID: {patient.Id}");
            sb.AppendLine($"Name: {patient.FullName}");
            sb.AppendLine($"Age: {patient.Age}");
            sb.AppendLine($"Last Diagnosis: {patient.LastDiagnosis ?? "None"}");
            sb.AppendLine("---");
        }

        return sb.ToString();
    }

    private string GetDefaultToken()
    {
        // For development/demo purposes - in production, this should be properly configured
        return "dev-token";
    }
}

// DTO classes for internal use
internal class PatientDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public int Age { get; set; }
    public string? LastDiagnosis { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

internal class PatientDetailDto : PatientDto
{
    public List<DiagnosticResultDto> DiagnosticResults { get; set; } = new();
}

internal class DiagnosticResultDto
{
    public Guid Id { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime TimestampUtc { get; set; }
}