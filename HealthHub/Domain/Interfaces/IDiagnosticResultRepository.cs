using HealthHub.Application.DTOs;
using HealthHub.Domain.Entities;

namespace HealthHub.Domain.Interfaces;

public interface IDiagnosticResultRepository
{
    Task<DiagnosticResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DiagnosticResult>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<DiagnosticResult> AddAsync(DiagnosticResult diagnosticResult, CancellationToken cancellationToken = default);
    Task UpdateAsync(DiagnosticResult diagnosticResult, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DiagnosticResult>> GetLatestByPatientAsync(Guid patientId, int count, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves diagnoses with optional filtering and pagination.
    /// </summary>
    /// <param name="filter">Filter criteria for diagnoses.</param>
    /// <param name="skip">Number of records to skip (for pagination).</param>
    /// <param name="take">Number of records to take (for pagination).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of diagnostic results matching the criteria.</returns>
    Task<IEnumerable<DiagnosticResult>> GetDiagnosesAsync(
        DiagnosisFilter filter,
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the total count of diagnoses matching the filter criteria.
    /// </summary>
    /// <param name="filter">Filter criteria for diagnoses.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total count of matching diagnoses.</returns>
    Task<int> GetDiagnosesCountAsync(
        DiagnosisFilter filter,
        CancellationToken cancellationToken = default);
}