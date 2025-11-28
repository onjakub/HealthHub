using HealthHub.Domain.ValueObjects;

namespace HealthHub.Domain.Entities;

public class Patient
{
    public Guid Id { get; private set; }
    public PatientName Name { get; private set; }
    public DateOnly DateOfBirth { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<DiagnosticResult> _diagnosticResults = new();
    public IReadOnlyCollection<DiagnosticResult> DiagnosticResults => _diagnosticResults.AsReadOnly();

    private Patient()
    {
        // EF Core constructor - initialize non-nullable properties
        Name = PatientName.Create("Unknown", "Patient");
        DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow);
        CreatedAt = DateTime.UtcNow;
    }

    public static Patient Create(PatientName name, DateOnly dateOfBirth)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));
        
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Date of birth cannot be in the future", nameof(dateOfBirth));

        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Name = name,
            DateOfBirth = dateOfBirth,
            CreatedAt = DateTime.UtcNow
        };

        return patient;
    }

    public void UpdateName(PatientName newName)
    {
        if (newName == null)
            throw new ArgumentNullException(nameof(newName));

        Name = newName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddDiagnosticResult(string diagnosis, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(diagnosis))
            throw new ArgumentException("Diagnosis cannot be empty", nameof(diagnosis));

        var diagnosticResult = DiagnosticResult.Create(this.Id, diagnosis, notes);
        _diagnosticResults.Add(diagnosticResult);
        UpdatedAt = DateTime.UtcNow;
    }

    public int GetAge()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - DateOfBirth.Year;
        if (today < DateOfBirth.AddYears(age)) age--;
        return age;
    }

    public string? GetLastDiagnosis()
    {
        return _diagnosticResults
            .OrderByDescending(d => d.TimestampUtc)
            .Select(d => d.Diagnosis.Value)
            .FirstOrDefault();
    }
}