using System;

namespace HealthHub.Application.DTOs;

/// <summary>
/// Filter criteria for retrieving diagnoses.
/// All properties are optional; null values are ignored.
/// </summary>
public record DiagnosisFilter
{
    /// <summary>
    /// Optional type/category of the diagnosis (e.g., "Chronic", "Acute").
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Optional flag indicating whether the diagnosis is active.
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Optional lower bound for the creation date (inclusive).
    /// </summary>
    public DateTime? CreatedAfter { get; init; }

    /// <summary>
    /// Optional upper bound for the creation date (inclusive).
    /// </summary>
    public DateTime? CreatedBefore { get; init; }
}