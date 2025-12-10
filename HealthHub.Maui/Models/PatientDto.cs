namespace HealthHub.Maui.Models;

public class PatientDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public int Age { get; set; }
    public string? LastDiagnosis { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string DisplayName => $"{FirstName} {LastName}";
    public string AgeDisplay => $"Age: {Age}";
    public string CreatedDisplay => $"Created: {CreatedAt:MM/dd/yyyy}";
}