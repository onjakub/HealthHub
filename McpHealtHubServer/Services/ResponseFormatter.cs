using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using McpHealtHubServer.Models;
using Microsoft.Extensions.Logging;

namespace McpHealtHubServer.Services;

public interface IResponseFormatter
{
    string FormatPatientList(IEnumerable<PatientDto> patients);
    string FormatPatientDetail(PatientDetailDto patient);
    string FormatDiagnosticResults(IEnumerable<DiagnosticResultDto> results);
    string FormatPatientSearch(IEnumerable<PatientDto> patients, string searchTerm, int? minAge, int? maxAge, bool? hasRecentDiagnosis);
    string FormatError(string message);
    string FormatSuccess(string message);
}

public class ResponseFormatter : IResponseFormatter
{
    private readonly ILogger<ResponseFormatter> _logger;

    public ResponseFormatter(ILogger<ResponseFormatter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string FormatPatientList(IEnumerable<PatientDto> patients)
    {
        _logger.LogDebug("Formatting patient list with {Count} patients", patients.Count());
        
        var patientList = patients.ToList();
        if (patientList.Count == 0)
        {
            _logger.LogInformation("No patients found to format");
            return "No patients found.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Found {patientList.Count} patients:");
        sb.AppendLine();

        foreach (var patient in patientList)
        {
            AppendPatientInfo(sb, patient);
            sb.AppendLine("---");
        }

        var formattedResult = sb.ToString();
        _logger.LogDebug("Formatted patient list successfully");
        
        return formattedResult;
    }

    public string FormatPatientDetail(PatientDetailDto patient)
    {
        _logger.LogDebug("Formatting patient detail for patient ID: {PatientId}", patient.Id);
        
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

        var formattedResult = sb.ToString();
        _logger.LogDebug("Formatted patient detail successfully");
        
        return formattedResult;
    }

    public string FormatDiagnosticResults(IEnumerable<DiagnosticResultDto> results)
    {
        _logger.LogDebug("Formatting diagnostic results with {Count} results", results.Count());
        
        var resultList = results.ToList();
        if (resultList.Count == 0)
        {
            _logger.LogInformation("No diagnostic results found to format");
            return "No diagnostic results found for this patient.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Found {resultList.Count} diagnostic results:");
        sb.AppendLine();

        foreach (var result in resultList.OrderByDescending(r => r.TimestampUtc))
        {
            sb.AppendLine($"Date: {result.TimestampUtc:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Diagnosis: {result.Diagnosis}");
            if (!string.IsNullOrEmpty(result.Notes))
            {
                sb.AppendLine($"Notes: {result.Notes}");
            }
            sb.AppendLine("---");
        }

        var formattedResult = sb.ToString();
        _logger.LogDebug("Formatted diagnostic results successfully");
        
        return formattedResult;
    }

    public string FormatPatientSearch(IEnumerable<PatientDto> patients, string searchTerm, int? minAge, int? maxAge, bool? hasRecentDiagnosis)
    {
        _logger.LogDebug("Formatting patient search results with {Count} patients", patients.Count());
        
        var patientList = patients.ToList();
        if (patientList.Count == 0)
        {
            _logger.LogInformation("No patients found matching search criteria");
            return "No patients found matching the search criteria.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Search Results:");
        sb.AppendLine($"- Search term: {searchTerm}");
        if (minAge.HasValue) sb.AppendLine($"- Minimum age: {minAge}");
        if (maxAge.HasValue) sb.AppendLine($"- Maximum age: {maxAge}");
        if (hasRecentDiagnosis.HasValue) sb.AppendLine($"- Has recent diagnosis: {hasRecentDiagnosis}");
        sb.AppendLine($"Found {patientList.Count} patients:");
        sb.AppendLine();

        foreach (var patient in patientList)
        {
            sb.AppendLine($"ID: {patient.Id}");
            sb.AppendLine($"Name: {patient.FullName}");
            sb.AppendLine($"Age: {patient.Age}");
            sb.AppendLine($"Last Diagnosis: {patient.LastDiagnosis ?? "None"}");
            sb.AppendLine("---");
        }

        var formattedResult = sb.ToString();
        _logger.LogDebug("Formatted patient search results successfully");
        
        return formattedResult;
    }

    public string FormatError(string message)
    {
        _logger.LogWarning("Formatting error message: {Message}", message);
        return $"Error: {message}";
    }

    public string FormatSuccess(string message)
    {
        _logger.LogDebug("Formatting success message: {Message}", message);
        return $"Success: {message}";
    }

    private void AppendPatientInfo(StringBuilder sb, PatientDto patient)
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
    }
}