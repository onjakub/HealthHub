using HealthHub.Application.DTOs;
using HealthHub.Application.Queries;
using HealthHub.Application.Handlers;
using HealthHub.Application.Services;
using HotChocolate;
using HotChocolate.Authorization;

namespace HealthHub.Presentation.GraphQL;

public class Query
{
    [Authorize]
    public async Task<PaginationResponseDto<PatientDto>> GetPatientsAsync(
        [Service] IQueryHandler<GetPatientsQuery, PaginationResponseDto<PatientDto>> handler,
        [Service] ILoggingService loggingService,
        string? searchTerm = null,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = loggingService.StartActivity("GetPatients");
        
        try
        {
            var query = new GetPatientsQuery
            {
                SearchTerm = searchTerm,
                Page = page,
                PageSize = pageSize
            };
            
            var result = await handler.Handle(query, cancellationToken);
            loggingService.LogInformation("Retrieved {Count} patients with search term: {SearchTerm}",
                result.Nodes.Count(), searchTerm ?? "none");
            return result;
        }
        catch (Exception ex)
        {
            loggingService.LogError(ex, "Failed to retrieve patients");
            throw;
        }
    }

    [Authorize]
    public async Task<PatientDetailDto?> GetPatientAsync(
        Guid id,
        [Service] IQueryHandler<GetPatientByIdQuery, PatientDetailDto?> handler,
        [Service] ILoggingService loggingService,
        CancellationToken cancellationToken)
    {
        using var activity = loggingService.StartActivity("GetPatientById");
        
        try
        {
            var query = new GetPatientByIdQuery { PatientId = id };
            var result = await handler.Handle(query, cancellationToken);
            
            if (result != null)
            {
                loggingService.LogInformation("Retrieved patient {PatientId} with {DiagnosisCount} diagnoses",
                    id, result.DiagnosticResults.Count);
            }
            else
            {
                loggingService.LogWarning("Patient not found: {PatientId}", id);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            loggingService.LogError(ex, "Failed to retrieve patient: {PatientId}", id);
            throw;
        }
    }

    [Authorize]
    public async Task<IEnumerable<DiagnosticResultDto>> GetPatientDiagnosticResultsAsync(
        Guid patientId,
        [Service] IQueryHandler<GetPatientDiagnosticResultsQuery, IEnumerable<DiagnosticResultDto>> handler,
        [Service] ILoggingService loggingService,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = loggingService.StartActivity("GetPatientDiagnosticResults");
        
        try
        {
            var query = new GetPatientDiagnosticResultsQuery
            {
                PatientId = patientId,
                Limit = limit
            };
            
            var result = await handler.Handle(query, cancellationToken);
            loggingService.LogInformation("Retrieved {Count} diagnostic results for patient {PatientId}",
                result.Count(), patientId);
            return result;
        }
        catch (Exception ex)
        {
            loggingService.LogError(ex, "Failed to retrieve diagnostic results for patient: {PatientId}", patientId);
            throw;
        }
    }
    [Authorize]
    public async Task<PaginationResponseDto<DiagnosticResultDto>> GetDiagnosesAsync(
        [Service] IQueryHandler<GetDiagnosesQuery, PaginationResponseDto<DiagnosticResultDto>> handler,
        [Service] ILoggingService loggingService,
        string? type = null,
        DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        bool? isActive = null,
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = loggingService.StartActivity("GetDiagnoses");
        
        try
        {
            var filter = new DiagnosisFilter
            {
                Type = type,
                CreatedAfter = createdAfter,
                CreatedBefore = createdBefore,
                IsActive = isActive
            };

            var query = new GetDiagnosesQuery
            {
                Filter = filter,
                Skip = skip,
                Take = take
            };
            
            var result = await handler.Handle(query, cancellationToken);
            loggingService.LogInformation("Retrieved {Count} diagnoses with filter: {Filter}",
                result.Nodes.Count(), filter);
            return result;
        }
        catch (Exception ex)
        {
            loggingService.LogError(ex, "Failed to retrieve diagnoses");
            throw;
        }
    }
}