using System;
using System.ComponentModel;
using System.Threading.Tasks;
using McpHealtHubServer.Exceptions;
using McpHealtHubServer.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace McpHealtHubServer.Tools;

/// <summary>
/// MCP tools for accessing patient information from HealthHub system.
/// This class provides MCP server tools that interact with the HealthHub GraphQL API
/// to retrieve patient data, diagnostic results, and perform patient searches.
/// </summary>
internal class PatientTools
{
    private readonly IPatientService _patientService;
    private readonly IResponseFormatter _responseFormatter;
    private readonly ILogger<PatientTools> _logger;

    public PatientTools(IPatientService patientService, IResponseFormatter responseFormatter, ILogger<PatientTools> logger)
    {
        _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
        _responseFormatter = responseFormatter ?? throw new ArgumentNullException(nameof(responseFormatter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [McpServerTool]
    [Description("Gets a list of patients with optional search and pagination.")]
    public async Task<string> GetPatients(
        [Description("Search term to filter patients by name or diagnosis")] string? searchTerm = null,
        [Description("Page number for pagination (starts at 1)")] int? page = null,
        [Description("Number of patients per page")] int? pageSize = null)
    {
        _logger.LogInformation("MCP tool GetPatients called with searchTerm: {SearchTerm}, page: {Page}, pageSize: {PageSize}",
            searchTerm, page, pageSize);
        
        try
        {
            var patients = await _patientService.GetPatientsAsync(searchTerm, page, pageSize);
            return _responseFormatter.FormatPatientList(patients);
        }
        catch (HealthHubValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error in GetPatients");
            return _responseFormatter.FormatError(ex.Message);
        }
        catch (HealthHubException ex)
        {
            _logger.LogError(ex, "Error in GetPatients");
            return _responseFormatter.FormatError("An error occurred while retrieving patients. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetPatients");
            return _responseFormatter.FormatError("An unexpected error occurred. Please try again.");
        }
    }

    [McpServerTool]
    [Description("Gets detailed information about a specific patient by ID.")]
    public async Task<string> GetPatientById(
        [Description("The unique identifier of the patient")] Guid patientId)
    {
        _logger.LogInformation("MCP tool GetPatientById called with patientId: {PatientId}", patientId);
        
        if (patientId == Guid.Empty)
        {
            _logger.LogWarning("Empty patient ID provided");
            return _responseFormatter.FormatError("Patient ID cannot be empty.");
        }

        try
        {
            var patient = await _patientService.GetPatientByIdAsync(patientId);
            
            if (patient == null)
            {
                _logger.LogInformation("Patient not found: {PatientId}", patientId);
                return "Patient not found.";
            }
            
            return _responseFormatter.FormatPatientDetail(patient);
        }
        catch (HealthHubValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error in GetPatientById");
            return _responseFormatter.FormatError(ex.Message);
        }
        catch (HealthHubException ex)
        {
            _logger.LogError(ex, "Error in GetPatientById");
            return _responseFormatter.FormatError("An error occurred while retrieving patient details. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetPatientById");
            return _responseFormatter.FormatError("An unexpected error occurred. Please try again.");
        }
    }

    [McpServerTool]
    [Description("Gets diagnostic results for a specific patient.")]
    public async Task<string> GetPatientDiagnosticResults(
        [Description("The unique identifier of the patient")] Guid patientId,
        [Description("Maximum number of results to return")] int? limit = null)
    {
        _logger.LogInformation("MCP tool GetPatientDiagnosticResults called with patientId: {PatientId}, limit: {Limit}", patientId, limit);
        
        if (patientId == Guid.Empty)
        {
            _logger.LogWarning("Empty patient ID provided");
            return _responseFormatter.FormatError("Patient ID cannot be empty.");
        }

        try
        {
            var results = await _patientService.GetPatientDiagnosticResultsAsync(patientId, limit);
            return _responseFormatter.FormatDiagnosticResults(results);
        }
        catch (HealthHubValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error in GetPatientDiagnosticResults");
            return _responseFormatter.FormatError(ex.Message);
        }
        catch (HealthHubException ex)
        {
            _logger.LogError(ex, "Error in GetPatientDiagnosticResults");
            return _responseFormatter.FormatError("An error occurred while retrieving diagnostic results. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetPatientDiagnosticResults");
            return _responseFormatter.FormatError("An unexpected error occurred. Please try again.");
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
        _logger.LogInformation("MCP tool SearchPatients called with searchTerm: {SearchTerm}, minAge: {MinAge}, maxAge: {MaxAge}, hasRecentDiagnosis: {HasRecentDiagnosis}",
            searchTerm, minAge, maxAge, hasRecentDiagnosis);
        
        try
        {
            var patients = await _patientService.SearchPatientsAsync(searchTerm, minAge, maxAge, hasRecentDiagnosis);
            return _responseFormatter.FormatPatientSearch(patients, searchTerm, minAge, maxAge, hasRecentDiagnosis);
        }
        catch (HealthHubValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error in SearchPatients");
            return _responseFormatter.FormatError(ex.Message);
        }
        catch (HealthHubException ex)
        {
            _logger.LogError(ex, "Error in SearchPatients");
            return _responseFormatter.FormatError("An error occurred while searching patients. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in SearchPatients");
            return _responseFormatter.FormatError("An unexpected error occurred. Please try again.");
        }
    }
}