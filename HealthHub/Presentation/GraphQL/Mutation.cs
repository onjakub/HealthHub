using HealthHub.Application.Commands;
using HealthHub.Application.DTOs;
using HealthHub.Application.Handlers;
using HealthHub.Application.Services;
using HotChocolate;
using HotChocolate.Authorization;

namespace HealthHub.Presentation.GraphQL;

public class Mutation
{
    [Authorize]
    public async Task<PatientDto> CreatePatientAsync(
        CreatePatientCommand command,
        [Service] ICommandHandler<CreatePatientCommand, PatientDto> handler,
        [Service] ILoggingService loggingService,
        CancellationToken cancellationToken)
    {
        using var activity = loggingService.StartActivity("CreatePatient");
        
        try
        {
            var result = await handler.Handle(command, cancellationToken);
            loggingService.LogAudit("CREATE", "Patient", result.Id.ToString(), "demo-user",
                $"Created patient: {result.FirstName} {result.LastName}");
            return result;
        }
        catch (Exception ex)
        {
            loggingService.LogError(ex, "Failed to create patient: {FirstName} {LastName}",
                command.FirstName, command.LastName);
            throw;
        }
    }

    [Authorize]
    public async Task<PatientDto> UpdatePatientAsync(
        UpdatePatientCommand command,
        [Service] ICommandHandler<UpdatePatientCommand, PatientDto> handler,
        [Service] ILoggingService loggingService,
        CancellationToken cancellationToken)
    {
        using var activity = loggingService.StartActivity("UpdatePatient");
        
        try
        {
            var result = await handler.Handle(command, cancellationToken);
            loggingService.LogAudit("UPDATE", "Patient", command.PatientId.ToString(), "demo-user",
                $"Updated patient: {result.FirstName} {result.LastName}");
            return result;
        }
        catch (Exception ex)
        {
            loggingService.LogError(ex, "Failed to update patient: {PatientId}", command.PatientId);
            throw;
        }
    }

    [Authorize]
    public async Task<DiagnosticResultDto> AddDiagnosticResultAsync(
        AddDiagnosticResultCommand command,
        [Service] ICommandHandler<AddDiagnosticResultCommand, DiagnosticResultDto> handler,
        [Service] ILoggingService loggingService,
        CancellationToken cancellationToken)
    {
        using var activity = loggingService.StartActivity("AddDiagnosticResult");
        
        try
        {
            var result = await handler.Handle(command, cancellationToken);
            loggingService.LogAudit("CREATE", "DiagnosticResult", result.Id.ToString(), "demo-user",
                $"Added diagnosis '{command.Diagnosis}' for patient {command.PatientId}");
            return result;
        }
        catch (Exception ex)
        {
            loggingService.LogError(ex, "Failed to add diagnostic result for patient: {PatientId}", command.PatientId);
            throw;
        }
    }

    [Authorize]
    public async Task<DiagnosticResultDto> UpdateDiagnosticResultAsync(
        UpdateDiagnosticResultCommand command,
        [Service] ICommandHandler<UpdateDiagnosticResultCommand, DiagnosticResultDto> handler,
        [Service] ILoggingService loggingService,
        CancellationToken cancellationToken)
    {
        using var activity = loggingService.StartActivity("UpdateDiagnosticResult");
        
        try
        {
            var result = await handler.Handle(command, cancellationToken);
            loggingService.LogAudit("UPDATE", "DiagnosticResult", command.DiagnosticResultId.ToString(), "demo-user",
                $"Updated diagnostic result for patient");
            return result;
        }
        catch (Exception ex)
        {
            loggingService.LogError(ex, "Failed to update diagnostic result: {DiagnosticResultId}", command.DiagnosticResultId);
            throw;
        }
    }

    [Authorize]
    public async Task<bool> DeletePatientAsync(
        DeletePatientCommand command,
        [Service] ICommandHandler<DeletePatientCommand, bool> handler,
        [Service] ILoggingService loggingService,
        CancellationToken cancellationToken)
    {
        using var activity = loggingService.StartActivity("DeletePatient");
        
        try
        {
            var result = await handler.Handle(command, cancellationToken);
            if (result)
            {
                loggingService.LogAudit("DELETE", "Patient", command.PatientId.ToString(), "demo-user",
                    "Deleted patient");
            }
            return result;
        }
        catch (Exception ex)
        {
            loggingService.LogError(ex, "Failed to delete patient: {PatientId}", command.PatientId);
            throw;
        }
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