namespace HealthHub.Maui.Models;

public class PatientDetailDto : PatientDto
{
    public List<DiagnosticResultDto> DiagnosticResults { get; set; } = new();
}

public class DiagnosticResultDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime TimestampUtc { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public string TimestampDisplay => TimestampUtc.ToLocalTime().ToString("MM/dd/yyyy HH:mm");
    public string NotesDisplay => string.IsNullOrEmpty(Notes) ? "No notes" : Notes;
}