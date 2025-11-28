using HealthHub.Application.DTOs;
using HealthHub.Application.Queries;
using HealthHub.Application.Handlers;
using HotChocolate;
using HotChocolate.Authorization;

namespace HealthHub.Presentation.GraphQL;

public class Query
{
    [Authorize]
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<PatientDto>> GetPatientsAsync(
        [Service] IQueryHandler<GetPatientsQuery, IEnumerable<PatientDto>> handler,
        string? searchTerm = null,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPatientsQuery
        {
            SearchTerm = searchTerm,
            Page = page,
            PageSize = pageSize
        };
        
        return await handler.Handle(query, cancellationToken);
    }

    [Authorize]
    public async Task<PatientDetailDto?> GetPatientAsync(
        Guid id,
        [Service] IQueryHandler<GetPatientByIdQuery, PatientDetailDto?> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetPatientByIdQuery { PatientId = id };
        return await handler.Handle(query, cancellationToken);
    }

    [Authorize]
    public async Task<IEnumerable<DiagnosticResultDto>> GetPatientDiagnosticResultsAsync(
        Guid patientId,
        [Service] IQueryHandler<GetPatientDiagnosticResultsQuery, IEnumerable<DiagnosticResultDto>> handler,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPatientDiagnosticResultsQuery
        {
            PatientId = patientId,
            Limit = limit
        };
        
        return await handler.Handle(query, cancellationToken);
    }
}