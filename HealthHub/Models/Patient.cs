namespace HealthHub.Models;

public class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }

    public ICollection<DiagnosticResult> DiagnosticResults { get; set; } = new List<DiagnosticResult>();

    public int Age
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - DateOfBirth.Year;
            if (today < DateOfBirth.AddYears(age)) age--;
            return age;
        }
    }

    public string? LastDiagnosis => DiagnosticResults
        .OrderByDescending(d => d.TimestampUtc)
        .Select(d => d.Diagnosis)
        .FirstOrDefault();
}
