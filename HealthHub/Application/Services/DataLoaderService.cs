using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthHub.Domain.Entities;
using HealthHub.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace HealthHub.Application.Services;

/// <summary>
/// Service for creating and managing DataLoader instances for GraphQL.
/// </summary>
public class DataLoaderService
{
    private readonly IServiceProvider _serviceProvider;

    public DataLoaderService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates a DataLoader for loading patients by IDs.
    /// </summary>
    public async Task<Dictionary<Guid, Patient?>> GetPatientsBatchAsync(IReadOnlyList<Guid> keys, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPatientRepository>();

        var patients = await repository.GetByIdsAsync(keys.ToList(), cancellationToken);
        return keys.ToDictionary(key => key, key => patients.FirstOrDefault(p => p.Id == key));
    }

    /// <summary>
    /// Creates a DataLoader for loading diagnostic results by patient IDs.
    /// </summary>
    public async Task<Dictionary<Guid, IReadOnlyList<DiagnosticResult>>> GetDiagnosticResultsByPatientBatchAsync(IReadOnlyList<Guid> keys, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IDiagnosticResultRepository>();

        var results = await repository.GetByPatientIdsAsync(keys.ToList(), cancellationToken);
        return keys.ToDictionary(key => key, key => (IReadOnlyList<DiagnosticResult>)results.Where(r => r.PatientId == key).ToList());
    }
}

/// <summary>
/// Extension methods for registering DataLoader services.
/// </summary>
public static class DataLoaderServiceExtensions
{
    public static IServiceCollection AddDataLoaderServices(this IServiceCollection services)
    {
        services.AddScoped<DataLoaderService>();
        return services;
    }
}