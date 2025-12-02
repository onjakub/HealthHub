using HealthHub.Application.DTOs;
using HealthHub.Application.Queries;
using HealthHub.Domain.Interfaces;

namespace HealthHub.Application.Handlers;

public class GetDiagnosesQueryHandler : IQueryHandler<GetDiagnosesQuery, IEnumerable<DiagnosticResultDto>>
{
    private readonly IDiagnosticResultRepository _diagnosticResultRepository;

    public GetDiagnosesQueryHandler(IDiagnosticResultRepository diagnosticResultRepository)
    {
        _diagnosticResultRepository = diagnosticResultRepository;
    }

    public async Task<IEnumerable<DiagnosticResultDto>> Handle(GetDiagnosesQuery query, CancellationToken cancellationToken)
    {
        var results = await _diagnosticResultRepository.GetDiagnosesAsync(
            query.Filter,
            query.Skip,
            query.Take,
            cancellationToken);

        return results.Select(r => new DiagnosticResultDto
        {
            Id = r.Id,
            PatientId = r.PatientId,
            Diagnosis = r.Diagnosis.Value,
            Notes = r.Notes,
            TimestampUtc = r.TimestampUtc,
            CreatedAt = r.CreatedAt
        });
    }
}