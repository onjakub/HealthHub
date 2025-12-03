using HealthHub.Application.DTOs;
using HealthHub.Application.Queries;
using HealthHub.Domain.Interfaces;

namespace HealthHub.Application.Handlers;

public class GetPatientsQueryHandler : IQueryHandler<GetPatientsQuery, PaginationResponseDto<PatientDto>>
{
    private readonly IPatientRepository _patientRepository;

    public GetPatientsQueryHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<PaginationResponseDto<PatientDto>> Handle(GetPatientsQuery query, CancellationToken cancellationToken)
    {
        // Use the new filtered method to avoid loading all patients into memory
        var filteredPatients = await _patientRepository.GetFilteredAsync(
            query.SearchTerm,
            query.Page,
            query.PageSize,
            cancellationToken);

        // Get total count for pagination
        var totalCount = await _patientRepository.GetFilteredCountAsync(
            query.SearchTerm,
            cancellationToken);

        var patientDtos = filteredPatients.Select(patient => new PatientDto
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
        }).ToList();

        var totalPages = query.PageSize.HasValue ? (int)Math.Ceiling((double)totalCount / query.PageSize.Value) : 1;
        var currentPage = query.Page ?? 1;

        return new PaginationResponseDto<PatientDto>
        {
            Nodes = patientDtos,
            PageInfo = new PageInfoDto
            {
                HasNextPage = currentPage < totalPages,
                HasPreviousPage = currentPage > 1,
                StartCursor = currentPage.ToString(),
                EndCursor = currentPage.ToString()
            },
            TotalCount = totalCount
        };
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