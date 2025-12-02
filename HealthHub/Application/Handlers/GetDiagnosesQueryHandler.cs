using HealthHub.Application.DTOs;
using HealthHub.Application.Queries;
using HealthHub.Domain.Interfaces;
using HealthHub.Domain.Entities;
using System.Linq;

namespace HealthHub.Application.Handlers;

public class GetDiagnosesQueryHandler : IQueryHandler<GetDiagnosesQuery, PaginationResponseDto<DiagnosticResultDto>>
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

    public async Task<PaginationResponseDto<DiagnosticResultDto>> Handle(GetDiagnosesQuery query, CancellationToken cancellationToken)
    {
        var results = await _diagnosticResultRepository.GetDiagnosesAsync(
            query.Filter,
            query.Skip,
            query.Take,
            cancellationToken);

        var totalCount = await _diagnosticResultRepository.GetDiagnosesCountAsync(
            query.Filter,
            cancellationToken);

        var diagnosticDtos = results.Select(result => new DiagnosticResultDto
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

        // Calculate pagination info
        var currentPage = query.Skip.HasValue && query.Take.HasValue
            ? (query.Skip.Value / query.Take.Value) + 1
            : 1;
        var pageSize = query.Take ?? totalCount;
        var totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 1;

        return new PaginationResponseDto<DiagnosticResultDto>
        {
            Nodes = diagnosticDtos,
            PageInfo = new PageInfoDto
            {
                HasNextPage = currentPage < totalPages,
                HasPreviousPage = currentPage > 1,
                StartCursor = query.Skip?.ToString(),
                EndCursor = (query.Skip ?? 0 + diagnosticDtos.Count()).ToString()
            },
            TotalCount = totalCount
        };
    }
}