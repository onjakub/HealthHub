using HealthHub.Application.DTOs;
using HealthHub.Application.Queries;
using HealthHub.Domain.Entities;
using HealthHub.Domain.Interfaces;

namespace HealthHub.Application.Handlers;

public class GetPatientDiagnosticResultsQueryHandler : IQueryHandler<GetPatientDiagnosticResultsQuery, IEnumerable<DiagnosticResultDto>>
{
    private readonly IDiagnosticResultRepository _diagnosticResultRepository;

    public GetPatientDiagnosticResultsQueryHandler(IDiagnosticResultRepository diagnosticResultRepository)
    {
        _diagnosticResultRepository = diagnosticResultRepository;
    }

    public async Task<IEnumerable<DiagnosticResultDto>> Handle(GetPatientDiagnosticResultsQuery query, CancellationToken cancellationToken)
    {
        IEnumerable<DiagnosticResult> diagnosticResults;

        if (query.Limit.HasValue)
        {
            diagnosticResults = await _diagnosticResultRepository.GetLatestByPatientAsync(
                query.PatientId, query.Limit.Value, cancellationToken);
        }
        else
        {
            diagnosticResults = await _diagnosticResultRepository.GetByPatientIdAsync(
                query.PatientId, cancellationToken);
        }

        return diagnosticResults.Select(dr => new DiagnosticResultDto
        {
            Id = dr.Id,
            PatientId = dr.PatientId,
            Diagnosis = dr.Diagnosis.Value,
            Notes = dr.Notes,
            TimestampUtc = dr.TimestampUtc,
            CreatedAt = dr.CreatedAt
        });
    }
}