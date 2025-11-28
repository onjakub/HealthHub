namespace HealthHub.Domain.ValueObjects;

public record Diagnosis
{
    public string Value { get; }

    private Diagnosis(string value)
    {
        Value = value;
    }

    public static Diagnosis Create(string diagnosis)
    {
        if (string.IsNullOrWhiteSpace(diagnosis))
            throw new ArgumentException("Diagnosis cannot be empty", nameof(diagnosis));

        if (diagnosis.Length > 500)
            throw new ArgumentException("Diagnosis cannot exceed 500 characters", nameof(diagnosis));

        return new Diagnosis(diagnosis.Trim());
    }
}