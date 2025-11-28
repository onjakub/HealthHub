using HealthHub.Domain.Entities;
using HealthHub.Domain.Interfaces;
using HealthHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthHub.Infrastructure.Repositories;

public class DiagnosticResultRepository : IDiagnosticResultRepository
{
    private readonly HealthHubDbContext _context;

    public DiagnosticResultRepository(HealthHubDbContext context)
    {
        _context = context;
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