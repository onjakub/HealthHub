using HealthHub.Application.DTOs;
using HealthHub.Application.Queries;
using HealthHub.Application.Services;
using HealthHub.Domain.Interfaces;
using HealthHub.Domain.Entities;
using System.Linq;

namespace HealthHub.Application.Handlers;

public class GetDiagnosesQueryHandler : IQueryHandler<GetDiagnosesQuery, PaginationResponseDto<DiagnosticResultDto>>
{
    private readonly IDiagnosticResultRepository _diagnosticResultRepository;
    private readonly DataLoaderService _dataLoaderService;

    public GetDiagnosesQueryHandler(
        IDiagnosticResultRepository diagnosticResultRepository,
        DataLoaderService dataLoaderService)
    {
        _diagnosticResultRepository = diagnosticResultRepository;
        _dataLoaderService = dataLoaderService;
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

        // Get unique patient IDs for batch loading
        var patientIds = results.Select(r => r.PatientId).Distinct().ToList();

        // Load all patients in a single batch
        var patients = await _dataLoaderService.GetPatientsBatchAsync(patientIds, cancellationToken);

        var diagnosticDtos = results.Select(result =>
        {
            var patient = patients.GetValueOrDefault(result.PatientId);

            return new DiagnosticResultDto
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
                    CreatedAt = patient.CreatedAt,
                    UpdatedAt = patient.UpdatedAt
                } : null,
                Diagnosis = result.Diagnosis.Value,
                Notes = result.Notes,
                TimestampUtc = result.TimestampUtc,
                CreatedAt = result.CreatedAt,
                IsActive = result.IsActive
            };
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
                EndCursor = (query.Skip ?? 0 + diagnosticDtos.Count).ToString()
            },
            TotalCount = totalCount
        };
    }
}