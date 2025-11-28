namespace HealthHub.Models;

public class DiagnosticResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string Diagnosis { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public Patient? Patient { get; set; }
}
