using HealthHub.Application.DTOs;
using HealthHub.Domain.Entities;
using HealthHub.Domain.Interfaces;
using HealthHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthHub.Infrastructure.Repositories;

public class DiagnosticResultRepository : IDiagnosticResultRepository
{
    private readonly HealthHubDbContext _context;
    private readonly ILogger<DiagnosticResultRepository> _logger;

    public DiagnosticResultRepository(
        HealthHubDbContext context,
        ILogger<DiagnosticResultRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<DiagnosticResult>> GetDiagnosesAsync(
        DiagnosisFilter filter,
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DiagnosticResults
            .Include(d => d.Patient)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.Type))
        {
            query = query.Where(d => d.Diagnosis.Value.Contains(filter.Type));
        }

        if (filter.CreatedAfter.HasValue)
        {
            query = query.Where(d => d.CreatedAt >= filter.CreatedAfter.Value);
        }

        if (filter.CreatedBefore.HasValue)
        {
            query = query.Where(d => d.CreatedAt <= filter.CreatedBefore.Value);
        }

        // Order by creation date (newest first)
        query = query.OrderByDescending(d => d.CreatedAt);

        // Apply pagination
        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        return await query
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<DiagnosticResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DiagnosticResults
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<DiagnosticResult>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _context.DiagnosticResults
            .Where(d => d.PatientId == patientId)
            .OrderByDescending(d => d.TimestampUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<DiagnosticResult> AddAsync(DiagnosticResult diagnosticResult, CancellationToken cancellationToken = default)
    {
        _context.DiagnosticResults.Add(diagnosticResult);
        await _context.SaveChangesAsync(cancellationToken);
        return diagnosticResult;
    }

    public async Task UpdateAsync(DiagnosticResult diagnosticResult, CancellationToken cancellationToken = default)
    {
        _context.DiagnosticResults.Update(diagnosticResult);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var diagnosticResult = await GetByIdAsync(id, cancellationToken);
        if (diagnosticResult != null)
        {
            _context.DiagnosticResults.Remove(diagnosticResult);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<DiagnosticResult>> GetLatestByPatientAsync(Guid patientId, int count, CancellationToken cancellationToken = default)
    {
        return await _context.DiagnosticResults
            .Where(d => d.PatientId == patientId)
            .OrderByDescending(d => d.TimestampUtc)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}