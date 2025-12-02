using HealthHub.Domain.ValueObjects;

namespace HealthHub.Domain.Entities;

public class DiagnosticResult
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public virtual Patient? Patient { get; private set; }
    public Diagnosis Diagnosis { get; private set; }
    public string? Notes { get; private set; }
    public DateTime TimestampUtc { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    private DiagnosticResult()
    {
        // EF Core constructor - initialize non-nullable properties
        Diagnosis = Diagnosis.Create("Unknown");
        TimestampUtc = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public static DiagnosticResult Create(Guid patientId, string diagnosis, string? notes = null)
    {
        if (patientId == Guid.Empty)
            throw new ArgumentException("Patient ID cannot be empty", nameof(patientId));

        if (string.IsNullOrWhiteSpace(diagnosis))
            throw new ArgumentException("Diagnosis cannot be empty", nameof(diagnosis));

        var diagnosticResult = new DiagnosticResult
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            Diagnosis = Diagnosis.Create(diagnosis),
            Notes = notes?.Trim(),
            TimestampUtc = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        return diagnosticResult;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
    }
}