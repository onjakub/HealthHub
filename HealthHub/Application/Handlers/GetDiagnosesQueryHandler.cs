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

        var resultDtos = new List<DiagnosticResultDto>();
        
        foreach (var result in results)
        {
            var patient = await _patientRepository.GetByIdAsync(result.PatientId, cancellationToken);
            
            resultDtos.Add(new DiagnosticResultDto
            {
                Id = result.Id,
                PatientId = result.PatientId,
                Patient = patient != null ? new PatientDto
                {
                    Id = patient.Id,
                    FirstName = patient.Name.FirstName,
                    LastName = patient.Name.LastName,
                    FullName = patient.Name.FullName,
                    DateOfBirth = patient.DateOfBirth,
                    Age = patient.GetAge(),
                    LastDiagnosis = patient.GetLastDiagnosis(),
                    CreatedAt = patient.CreatedAt
                } : null,
                Diagnosis = result.Diagnosis.Value,
                Notes = result.Notes,
                TimestampUtc = result.TimestampUtc,
                CreatedAt = result.CreatedAt,
                IsActive = result.IsActive
            });
        }
        
        return resultDtos;
    }
}