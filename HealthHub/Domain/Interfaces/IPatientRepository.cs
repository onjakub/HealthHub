using HealthHub.Domain.Entities;

namespace HealthHub.Domain.Interfaces;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Patient>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Patient> AddAsync(Patient patient, CancellationToken cancellationToken = default);
    Task UpdateAsync(Patient patient, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves filtered and paginated patients based on search criteria.
    /// </summary>
    /// <param name="searchTerm">Optional search term to filter patients by name or diagnosis.</param>
    /// <param name="page">Page number for pagination (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Filtered and paginated collection of patients.</returns>
    Task<IEnumerable<Patient>> GetFilteredAsync(
        string? searchTerm,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of patients matching the search criteria.
    /// </summary>
    /// <param name="searchTerm">Optional search term to filter patients by name or diagnosis.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total count of matching patients.</returns>
    Task<int> GetFilteredCountAsync(
        string? searchTerm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves patients by their IDs.
    /// </summary>
    /// <param name="ids">List of patient IDs to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of patients matching the provided IDs.</returns>
    Task<IEnumerable<Patient>> GetByIdsAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken = default);
}