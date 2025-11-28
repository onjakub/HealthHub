using HealthHub.Application.DTOs;

namespace HealthHub.Application.Commands;

public record CreatePatientCommand : ICommand<PatientDto>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
}

public record UpdatePatientCommand : ICommand<PatientDto>
{
    public Guid PatientId { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public DateOnly? DateOfBirth { get; init; }
}

public record AddDiagnosticResultCommand : ICommand<DiagnosticResultDto>
{
    public Guid PatientId { get; init; }
    public string Diagnosis { get; init; } = string.Empty;
    public string? Notes { get; init; }
}

public record UpdateDiagnosticResultCommand : ICommand<DiagnosticResultDto>
{
    public Guid DiagnosticResultId { get; init; }
    public string? Diagnosis { get; init; }
    public string? Notes { get; init; }
}

public record DeletePatientCommand : ICommand<bool>
{
    public Guid PatientId { get; init; }
}

public interface ICommand<TResponse> { }