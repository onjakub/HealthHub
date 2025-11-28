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
}