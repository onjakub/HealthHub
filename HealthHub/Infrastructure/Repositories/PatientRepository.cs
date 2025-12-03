using Microsoft.Extensions.Logging;
using HealthHub.Domain.Entities;
using HealthHub.Domain.Interfaces;
using HealthHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthHub.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly HealthHubDbContext _context;
    private readonly ILogger<PatientRepository> _logger;

    public PatientRepository(HealthHubDbContext context, ILogger<PatientRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Patients
            .Include(p => p.DiagnosticResults)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Patient>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Patients
            .Include(p => p.DiagnosticResults)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Patient> AddAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync(cancellationToken);
        return patient;
    }

    public async Task UpdateAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        _context.Patients.Update(patient);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var patient = await GetByIdAsync(id, cancellationToken);
        if (patient != null)
        {
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Patients.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Patients.CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Patient>> GetFilteredAsync(
        string? searchTerm,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Patients
            .Include(p => p.DiagnosticResults)
            .AsNoTracking()
            .AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            _logger.LogDebug("Applying search filter with term: {SearchTerm}", searchTerm);
            
            // Use EF.Functions.ILike for PostgreSQL case-insensitive search
            // This replaces string.Contains with StringComparison.OrdinalIgnoreCase which EF Core cannot translate
            query = query.Where(p =>
                EF.Functions.ILike(p.Name.FirstName, $"%{searchTerm}%") ||
                EF.Functions.ILike(p.Name.LastName, $"%{searchTerm}%") ||
                p.DiagnosticResults.Any(d =>
                    EF.Functions.ILike(d.Diagnosis.Value, $"%{searchTerm}%"))
            );
        }

        // Apply pagination if provided
        if (page.HasValue && pageSize.HasValue)
        {
            var skip = (page.Value - 1) * pageSize.Value;
            query = query.Skip(skip).Take(pageSize.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<int> GetFilteredCountAsync(
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Patients.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            _logger.LogDebug("Applying count search filter with term: {SearchTerm}", searchTerm);
            
            // Use EF.Functions.ILike for PostgreSQL case-insensitive search
            // This replaces string.Contains with StringComparison.OrdinalIgnoreCase which EF Core cannot translate
            query = query.Where(p =>
                EF.Functions.ILike(p.Name.FirstName, $"%{searchTerm}%") ||
                EF.Functions.ILike(p.Name.LastName, $"%{searchTerm}%") ||
                p.DiagnosticResults.Any(d =>
                    EF.Functions.ILike(d.Diagnosis.Value, $"%{searchTerm}%"))
            );
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Patient>> GetByIdsAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        return await _context.Patients
            .Include(p => p.DiagnosticResults)
            .Where(p => ids.Contains(p.Id))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}