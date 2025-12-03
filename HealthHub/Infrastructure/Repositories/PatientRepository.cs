using HealthHub.Domain.Entities;
using HealthHub.Domain.Interfaces;
using HealthHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthHub.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly HealthHubDbContext _context;

    public PatientRepository(HealthHubDbContext context)
    {
        _context = context;
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
            query = query.Where(p =>
                p.Name.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Name.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.DiagnosticResults.Any(d =>
                    d.Diagnosis.Value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
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
            query = query.Where(p =>
                p.Name.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Name.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.DiagnosticResults.Any(d =>
                    d.Diagnosis.Value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
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