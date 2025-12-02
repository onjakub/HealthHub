using HealthHub.Application.DTOs;
using HealthHub.Application.Queries;

namespace HealthHub.Application.Queries;

public record GetDiagnosesQuery : IQuery<IEnumerable<DiagnosticResultDto>>
{
    public DiagnosisFilter Filter { get; init; } = new();
    public int? Skip { get; init; }
    public int? Take { get; init; }
}