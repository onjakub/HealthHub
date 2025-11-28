using HealthHub.Application.Commands;
using HealthHub.Application.DTOs;
using HealthHub.Application.Handlers;
using HotChocolate;
using HotChocolate.Authorization;

namespace HealthHub.Presentation.GraphQL;

public class Mutation
{
    [Authorize]
    public async Task<PatientDto> CreatePatientAsync(
        CreatePatientCommand command,
        [Service] ICommandHandler<CreatePatientCommand, PatientDto> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(command, cancellationToken);
    }

    [Authorize]
    public async Task<PatientDto> UpdatePatientAsync(
        UpdatePatientCommand command,
        [Service] ICommandHandler<UpdatePatientCommand, PatientDto> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(command, cancellationToken);
    }

    [Authorize]
    public async Task<DiagnosticResultDto> AddDiagnosticResultAsync(
        AddDiagnosticResultCommand command,
        [Service] ICommandHandler<AddDiagnosticResultCommand, DiagnosticResultDto> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(command, cancellationToken);
    }

    [Authorize]
    public async Task<DiagnosticResultDto> UpdateDiagnosticResultAsync(
        UpdateDiagnosticResultCommand command,
        [Service] ICommandHandler<UpdateDiagnosticResultCommand, DiagnosticResultDto> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(command, cancellationToken);
    }

    [Authorize]
    public async Task<bool> DeletePatientAsync(
        DeletePatientCommand command,
        [Service] ICommandHandler<DeletePatientCommand, bool> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(command, cancellationToken);
    }
}

// GraphQL Input Types
public record CreatePatientInput(string FirstName, string LastName, DateOnly DateOfBirth)
{
    public CreatePatientCommand ToCommand() => new()
    {
        FirstName = FirstName,
        LastName = LastName,
        DateOfBirth = DateOfBirth
    };
}

public record UpdatePatientInput(Guid PatientId, string? FirstName, string? LastName, DateOnly? DateOfBirth)
{
    public UpdatePatientCommand ToCommand() => new()
    {
        PatientId = PatientId,
        FirstName = FirstName,
        LastName = LastName,
        DateOfBirth = DateOfBirth
    };
}

public record AddDiagnosticResultInput(Guid PatientId, string Diagnosis, string? Notes)
{
    public AddDiagnosticResultCommand ToCommand() => new()
    {
        PatientId = PatientId,
        Diagnosis = Diagnosis,
        Notes = Notes
    };
}

public record UpdateDiagnosticResultInput(Guid DiagnosticResultId, string? Diagnosis, string? Notes)
{
    public UpdateDiagnosticResultCommand ToCommand() => new()
    {
        DiagnosticResultId = DiagnosticResultId,
        Diagnosis = Diagnosis,
        Notes = Notes
    };
}

public record DeletePatientInput(Guid PatientId)
{
    public DeletePatientCommand ToCommand() => new()
    {
        PatientId = PatientId
    };
}