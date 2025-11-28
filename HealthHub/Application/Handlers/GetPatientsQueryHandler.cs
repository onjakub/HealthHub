using HealthHub.Application.DTOs;
using HealthHub.Application.Queries;
using HealthHub.Domain.Interfaces;

namespace HealthHub.Application.Handlers;

public class GetPatientsQueryHandler : IQueryHandler<GetPatientsQuery, IEnumerable<PatientDto>>
{
    private readonly IPatientRepository _patientRepository;

    public GetPatientsQueryHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<IEnumerable<PatientDto>> Handle(GetPatientsQuery query, CancellationToken cancellationToken)
    {
        var patients = await _patientRepository.GetAllAsync(cancellationToken);

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            patients = patients.Where(p => 
                p.Name.FirstName.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Name.LastName.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.GetLastDiagnosis()?.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) == true
            );
        }

        // Apply pagination if provided
        if (query.Page.HasValue && query.PageSize.HasValue)
        {
            var skip = (query.Page.Value - 1) * query.PageSize.Value;
            patients = patients.Skip(skip).Take(query.PageSize.Value);
        }

        return patients.Select(patient => new PatientDto
        {
            Id = patient.Id,
            FirstName = patient.Name.FirstName,
            LastName = patient.Name.LastName,
            FullName = patient.Name.FullName,
            DateOfBirth = patient.DateOfBirth,
            Age = patient.GetAge(),
            LastDiagnosis = patient.GetLastDiagnosis(),
            CreatedAt = patient.CreatedAt,
            UpdatedAt = patient.UpdatedAt
        });
    }
}

public class GetPatientByIdQueryHandler : IQueryHandler<GetPatientByIdQuery, PatientDetailDto?>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IDiagnosticResultRepository _diagnosticResultRepository;

    public GetPatientByIdQueryHandler(
        IPatientRepository patientRepository,
        IDiagnosticResultRepository diagnosticResultRepository)
    {
        _patientRepository = patientRepository;
        _diagnosticResultRepository = diagnosticResultRepository;
    }

    public async Task<PatientDetailDto?> Handle(GetPatientByIdQuery query, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(query.PatientId, cancellationToken);
        if (patient == null)
            return null;

        var diagnosticResults = await _diagnosticResultRepository.GetByPatientIdAsync(query.PatientId, cancellationToken);

        var patientDto = new PatientDetailDto
        {
            Id = patient.Id,
            FirstName = patient.Name.FirstName,
            LastName = patient.Name.LastName,
            FullName = patient.Name.FullName,
            DateOfBirth = patient.DateOfBirth,
            Age = patient.GetAge(),
            LastDiagnosis = patient.GetLastDiagnosis(),
            CreatedAt = patient.CreatedAt,
            UpdatedAt = patient.UpdatedAt,
            DiagnosticResults = diagnosticResults.Select(dr => new DiagnosticResultDto
            {
                Id = dr.Id,
                PatientId = dr.PatientId,
                Diagnosis = dr.Diagnosis.Value,
                Notes = dr.Notes,
                TimestampUtc = dr.TimestampUtc,
                CreatedAt = dr.CreatedAt
            }).ToList()
        };

        return patientDto;
    }
}

public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
}