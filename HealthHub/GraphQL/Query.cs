using HealthHub.Data;
using HealthHub.Models;
using HotChocolate;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthHub.GraphQL;

public class Query
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Patient> GetPatients([Service] HealthHubDbContext db)
        => db.Patients.AsNoTracking();

    public async Task<Patient?> GetPatientAsync(Guid id, [Service] HealthHubDbContext db, CancellationToken ct)
        => await db.Patients
            .AsNoTracking()
            .Include(p => p.DiagnosticResults)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
}
