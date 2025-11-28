using HealthHub.Application.DTOs;

namespace HealthHub.Application.Queries;

public record GetPatientsQuery : IQuery<IEnumerable<PatientDto>>
{
    public string? SearchTerm { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}

public record GetPatientByIdQuery : IQuery<PatientDetailDto?>
{
    public Guid PatientId { get; init; }
}

public record GetPatientDiagnosticResultsQuery : IQuery<IEnumerable<DiagnosticResultDto>>
{
    public Guid PatientId { get; init; }
    public int? Limit { get; init; }
}

public interface IQuery<TResponse> { }