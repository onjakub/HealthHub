namespace HealthHub.Application.DTOs;

public class DiagnosticResultDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public PatientDto? Patient { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime TimestampUtc { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateDiagnosticResultDto
{
    public Guid PatientId { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpdateDiagnosticResultDto
{
    public string? Diagnosis { get; set; }
    public string? Notes { get; set; }
}