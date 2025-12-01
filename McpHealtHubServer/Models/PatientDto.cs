using System;
using System.Collections.Generic;

namespace McpHealtHubServer.Models;

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
    public DateTime? UpdatedAt { get; set; }
}

public class PatientDetailDto : PatientDto
{
    public List<DiagnosticResultDto> DiagnosticResults { get; set; } = new();
}

public class DiagnosticResultDto
{
    public Guid Id { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime TimestampUtc { get; set; }
}