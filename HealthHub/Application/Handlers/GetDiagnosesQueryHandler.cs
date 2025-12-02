using HealthHub.Application.DTOs;
using HealthHub.Application.Queries;
using HealthHub.Domain.Interfaces;
using HealthHub.Domain.Entities;
using System.Linq;

namespace HealthHub.Application.Handlers;

public class GetDiagnosesQueryHandler : IQueryHandler<GetDiagnosesQuery, IEnumerable<DiagnosticResultDto>>
{
    private readonly IDiagnosticResultRepository _diagnosticResultRepository;
    private readonly IPatientRepository _patientRepository;

    public GetDiagnosesQueryHandler(
        IDiagnosticResultRepository diagnosticResultRepository,
        IPatientRepository patientRepository)
    {
        _diagnosticResultRepository = diagnosticResultRepository;
        _patientRepository = patientRepository;
    }

    public async Task<IEnumerable<DiagnosticResultDto>> Handle(GetDiagnosesQuery query, CancellationToken cancellationToken)
    {
        var results = await _diagnosticResultRepository.GetDiagnosesAsync(
            query.Filter,
            query.Skip,
            query.Take,
            cancellationToken);

        return results.Select(result => new DiagnosticResultDto
        {
            Id = result.Id,
            PatientId = result.PatientId,
            Patient = result.Patient != null ? new PatientDto
            {
                Id = result.Patient.Id,
                FirstName = result.Patient.Name.FirstName,
                LastName = result.Patient.Name.LastName,
                FullName = result.Patient.Name.FullName,
                DateOfBirth = result.Patient.DateOfBirth,
                Age = result.Patient.GetAge(),
                LastDiagnosis = result.Patient.GetLastDiagnosis(),
                CreatedAt = result.Patient.CreatedAt
            } : null,
            Diagnosis = result.Diagnosis.Value,
            Notes = result.Notes,
            TimestampUtc = result.TimestampUtc,
            CreatedAt = result.CreatedAt,
            IsActive = result.IsActive
        }).ToList();
    }
}