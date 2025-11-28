using HealthHub.Application.Commands;
using HealthHub.Application.DTOs;
using HealthHub.Domain.Entities;
using HealthHub.Domain.Interfaces;

namespace HealthHub.Application.Handlers;

public class AddDiagnosticResultCommandHandler : ICommandHandler<AddDiagnosticResultCommand, DiagnosticResultDto>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IDiagnosticResultRepository _diagnosticResultRepository;

    public AddDiagnosticResultCommandHandler(
        IPatientRepository patientRepository,
        IDiagnosticResultRepository diagnosticResultRepository)
    {
        _patientRepository = patientRepository;
        _diagnosticResultRepository = diagnosticResultRepository;
    }

    public async Task<DiagnosticResultDto> Handle(AddDiagnosticResultCommand command, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(command.PatientId, cancellationToken);
        if (patient == null)
            throw new ArgumentException($"Patient with ID {command.PatientId} not found");

        var diagnosticResult = DiagnosticResult.Create(command.PatientId, command.Diagnosis, command.Notes);
        var createdResult = await _diagnosticResultRepository.AddAsync(diagnosticResult, cancellationToken);

        return new DiagnosticResultDto
        {
            Id = createdResult.Id,
            PatientId = createdResult.PatientId,
            Diagnosis = createdResult.Diagnosis.Value,
            Notes = createdResult.Notes,
            TimestampUtc = createdResult.TimestampUtc,
            CreatedAt = createdResult.CreatedAt
        };
    }
}

public class UpdateDiagnosticResultCommandHandler : ICommandHandler<UpdateDiagnosticResultCommand, DiagnosticResultDto>
{
    private readonly IDiagnosticResultRepository _diagnosticResultRepository;

    public UpdateDiagnosticResultCommandHandler(IDiagnosticResultRepository diagnosticResultRepository)
    {
        _diagnosticResultRepository = diagnosticResultRepository;
    }

    public async Task<DiagnosticResultDto> Handle(UpdateDiagnosticResultCommand command, CancellationToken cancellationToken)
    {
        var diagnosticResult = await _diagnosticResultRepository.GetByIdAsync(command.DiagnosticResultId, cancellationToken);
        if (diagnosticResult == null)
            throw new ArgumentException($"Diagnostic result with ID {command.DiagnosticResultId} not found");

        if (!string.IsNullOrWhiteSpace(command.Notes))
        {
            diagnosticResult.UpdateNotes(command.Notes);
        }

        await _diagnosticResultRepository.UpdateAsync(diagnosticResult, cancellationToken);

        return new DiagnosticResultDto
        {
            Id = diagnosticResult.Id,
            PatientId = diagnosticResult.PatientId,
            Diagnosis = diagnosticResult.Diagnosis.Value,
            Notes = diagnosticResult.Notes,
            TimestampUtc = diagnosticResult.TimestampUtc,
            CreatedAt = diagnosticResult.CreatedAt
        };
    }
}

public class DeletePatientCommandHandler : ICommandHandler<DeletePatientCommand, bool>
{
    private readonly IPatientRepository _patientRepository;

    public DeletePatientCommandHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<bool> Handle(DeletePatientCommand command, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(command.PatientId, cancellationToken);
        if (patient == null)
            return false;

        await _patientRepository.DeleteAsync(command.PatientId, cancellationToken);
        return true;
    }
}