using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using McpHealtHubServer.Configuration;
using McpHealtHubServer.Exceptions;
using McpHealtHubServer.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace McpHealtHubServer.Services;

public interface IPatientService
{
    Task<IEnumerable<PatientDto>> GetPatientsAsync(string? searchTerm = null, int? page = null, int? pageSize = null);
    Task<PatientDetailDto?> GetPatientByIdAsync(Guid patientId);
    Task<IEnumerable<DiagnosticResultDto>> GetPatientDiagnosticResultsAsync(Guid patientId, int? limit = null);
    Task<IEnumerable<PatientDto>> SearchPatientsAsync(string searchTerm, int? minAge = null, int? maxAge = null, bool? hasRecentDiagnosis = null);
}

public class PatientService : IPatientService
{
    private readonly IHealthHubGraphQLClient _graphQLClient;
    private readonly IResponseFormatter _responseFormatter;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PatientService> _logger;
    private readonly HealthHubConfiguration _configuration;

    public PatientService(
        IHealthHubGraphQLClient graphQLClient,
        IResponseFormatter responseFormatter,
        IMemoryCache cache,
        ILogger<PatientService> logger,
        HealthHubConfiguration configuration)
    {
        _graphQLClient = graphQLClient ?? throw new ArgumentNullException(nameof(graphQLClient));
        _responseFormatter = responseFormatter ?? throw new ArgumentNullException(nameof(responseFormatter));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<IEnumerable<PatientDto>> GetPatientsAsync(string? searchTerm = null, int? page = null, int? pageSize = null)
    {
        _logger.LogInformation("Getting patients with searchTerm: {SearchTerm}, page: {Page}, pageSize: {PageSize}", 
            searchTerm, page, pageSize);
        
        var cacheKey = $"patients_{searchTerm}_{page}_{pageSize}";
        if (_cache.TryGetValue(cacheKey, out IEnumerable<PatientDto>? cachedPatients))
        {
            _logger.LogDebug("Returning cached patients for key: {CacheKey}", cacheKey);
            return cachedPatients!;
        }

        try
        {
            var query = @"
                query GetPatients($searchTerm: String, $page: Int, $pageSize: Int) {
                    getPatientsAsync(searchTerm: $searchTerm, page: $page, pageSize: $pageSize) {
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
                }";

            var variables = new { searchTerm, page, pageSize };
            var response = await _graphQLClient.SendQueryAsync(query, variables);
            
            var patients = ParsePatientListFromResponse(response, "getPatientsAsync");
            
            _cache.Set(cacheKey, patients, TimeSpan.FromMinutes(_configuration.CacheExpirationMinutes));
            _logger.LogDebug("Cached patients for key: {CacheKey}", cacheKey);
            
            return patients;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patients");
            throw new HealthHubException("Error retrieving patients", ex);
        }
    }

    public async Task<PatientDetailDto?> GetPatientByIdAsync(Guid patientId)
    {
        _logger.LogInformation("Getting patient by ID: {PatientId}", patientId);
        
        if (patientId == Guid.Empty)
        {
            throw new HealthHubValidationException("Patient ID cannot be empty");
        }

        var cacheKey = $"patient_{patientId}";
        if (_cache.TryGetValue(cacheKey, out PatientDetailDto? cachedPatient))
        {
            _logger.LogDebug("Returning cached patient for key: {CacheKey}", cacheKey);
            return cachedPatient;
        }

        try
        {
            var query = @"
                query GetPatient($id: UUID!) {
                    getPatientAsync(id: $id) {
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
                }";

            var variables = new { id = patientId };
            var response = await _graphQLClient.SendQueryAsync(query, variables);
            
            var patient = ParsePatientDetailFromResponse(response);
            
            if (patient != null)
            {
                _cache.Set(cacheKey, patient, TimeSpan.FromMinutes(_configuration.CacheExpirationMinutes));
                _logger.LogDebug("Cached patient for key: {CacheKey}", cacheKey);
            }
            
            return patient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient by ID: {PatientId}", patientId);
            throw new HealthHubException($"Error retrieving patient with ID {patientId}", ex);
        }
    }

    public async Task<IEnumerable<DiagnosticResultDto>> GetPatientDiagnosticResultsAsync(Guid patientId, int? limit = null)
    {
        _logger.LogInformation("Getting diagnostic results for patient ID: {PatientId}, limit: {Limit}", patientId, limit);
        
        if (patientId == Guid.Empty)
        {
            throw new HealthHubValidationException("Patient ID cannot be empty");
        }

        if (limit.HasValue && limit.Value <= 0)
        {
            throw new HealthHubValidationException("Limit must be a positive number");
        }

        var cacheKey = $"diagnostic_results_{patientId}_{limit}";
        if (_cache.TryGetValue(cacheKey, out IEnumerable<DiagnosticResultDto>? cachedResults))
        {
            _logger.LogDebug("Returning cached diagnostic results for key: {CacheKey}", cacheKey);
            return cachedResults!;
        }

        try
        {
            var query = @"
                query GetPatientDiagnosticResults($patientId: UUID!, $limit: Int) {
                    getPatientDiagnosticResultsAsync(patientId: $patientId, limit: $limit) {
                        id
                        diagnosis
                        notes
                        timestampUtc
                    }
                }";

            var variables = new { patientId, limit };
            var response = await _graphQLClient.SendQueryAsync(query, variables);
            
            var results = ParseDiagnosticResultsFromResponse(response);
            
            _cache.Set(cacheKey, results, TimeSpan.FromMinutes(_configuration.CacheExpirationMinutes));
            _logger.LogDebug("Cached diagnostic results for key: {CacheKey}", cacheKey);
            
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting diagnostic results for patient ID: {PatientId}", patientId);
            throw new HealthHubException($"Error retrieving diagnostic results for patient {patientId}", ex);
        }
    }

    public async Task<IEnumerable<PatientDto>> SearchPatientsAsync(string searchTerm, int? minAge = null, int? maxAge = null, bool? hasRecentDiagnosis = null)
    {
        _logger.LogInformation("Searching patients with term: {SearchTerm}, minAge: {MinAge}, maxAge: {MaxAge}, hasRecentDiagnosis: {HasRecentDiagnosis}", 
            searchTerm, minAge, maxAge, hasRecentDiagnosis);
        
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            throw new HealthHubValidationException("Search term is required");
        }

        ValidateSearchParameters(minAge, maxAge);

        var cacheKey = $"search_{searchTerm}_{minAge}_{maxAge}_{hasRecentDiagnosis}";
        if (_cache.TryGetValue(cacheKey, out IEnumerable<PatientDto>? cachedPatients))
        {
            _logger.LogDebug("Returning cached search results for key: {CacheKey}", cacheKey);
            return cachedPatients!;
        }

        try
        {
            // First get all patients matching the search term
            var allPatients = await GetPatientsAsync(searchTerm);
            
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

            var result = filteredPatients.ToList();
            
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_configuration.CacheExpirationMinutes));
            _logger.LogDebug("Cached search results for key: {CacheKey}", cacheKey);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching patients with term: {SearchTerm}", searchTerm);
            throw new HealthHubException($"Error searching patients with term '{searchTerm}'", ex);
        }
    }

    private void ValidateSearchParameters(int? minAge, int? maxAge)
    {
        var validationErrors = new List<string>();

        if (minAge.HasValue && minAge.Value < 0)
        {
            validationErrors.Add("Minimum age cannot be negative");
        }

        if (maxAge.HasValue && maxAge.Value < 0)
        {
            validationErrors.Add("Maximum age cannot be negative");
        }

        if (minAge.HasValue && maxAge.HasValue && minAge.Value > maxAge.Value)
        {
            validationErrors.Add("Minimum age cannot be greater than maximum age");
        }

        if (validationErrors.Any())
        {
            throw new HealthHubValidationException(validationErrors);
        }
    }

    private List<PatientDto> ParsePatientListFromResponse(JsonDocument response, string fieldName)
    {
        _logger.LogDebug("Parsing patient list from response for field: {FieldName}", fieldName);
        
        var data = response.RootElement.GetProperty("data").GetProperty(fieldName);
        var patients = new List<PatientDto>();

        foreach (var patientElement in data.EnumerateArray())
        {
            patients.Add(ParsePatientFromElement(patientElement));
        }

        _logger.LogDebug("Parsed {Count} patients from response", patients.Count);
        
        return patients;
    }

    private PatientDetailDto? ParsePatientDetailFromResponse(JsonDocument response)
    {
        _logger.LogDebug("Parsing patient detail from response");
        
        var data = response.RootElement.GetProperty("data").GetProperty("getPatientAsync");

        if (data.ValueKind == JsonValueKind.Null)
        {
            _logger.LogInformation("Patient not found in response");
            return null;
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
            patient.DiagnosticResults.Add(ParseDiagnosticResultFromElement(resultElement));
        }

        _logger.LogDebug("Parsed patient detail with {Count} diagnostic results", patient.DiagnosticResults.Count);
        
        return patient;
    }

    private List<DiagnosticResultDto> ParseDiagnosticResultsFromResponse(JsonDocument response)
    {
        _logger.LogDebug("Parsing diagnostic results from response");
        
        var data = response.RootElement.GetProperty("data").GetProperty("getPatientDiagnosticResultsAsync");
        var results = new List<DiagnosticResultDto>();

        foreach (var resultElement in data.EnumerateArray())
        {
            results.Add(ParseDiagnosticResultFromElement(resultElement));
        }

        _logger.LogDebug("Parsed {Count} diagnostic results from response", results.Count);
        
        return results;
    }

    private PatientDto ParsePatientFromElement(JsonElement patientElement)
    {
        return new PatientDto
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
        };
    }

    private DiagnosticResultDto ParseDiagnosticResultFromElement(JsonElement resultElement)
    {
        return new DiagnosticResultDto
        {
            Id = resultElement.GetProperty("id").GetGuid(),
            Diagnosis = resultElement.GetProperty("diagnosis").GetString() ?? "",
            Notes = resultElement.GetProperty("notes").GetString(),
            TimestampUtc = DateTime.Parse(resultElement.GetProperty("timestampUtc").GetString() ?? "")
        };
    }
}