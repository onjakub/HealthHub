using HealthHub.Domain.Entities;
using HealthHub.Domain.ValueObjects;
using Xunit;

namespace Domain.Tests;

public class PatientTests
{
    [Fact]
    public void CreatePatient_WithValidData_ShouldCreatePatient()
    {
        // Arrange
        var name = PatientName.Create("Jan", "Novák");
        var dateOfBirth = new DateOnly(1980, 1, 1);

        // Act
        var patient = Patient.Create(name, dateOfBirth);

        // Assert
        Assert.NotNull(patient);
        Assert.NotEqual(Guid.Empty, patient.Id);
        Assert.Equal(name, patient.Name);
        Assert.Equal(dateOfBirth, patient.DateOfBirth);
        Assert.NotNull(patient.DiagnosticResults);
        Assert.Empty(patient.DiagnosticResults);
    }

    [Fact]
    public void CreatePatient_WithFutureDateOfBirth_ShouldThrowException()
    {
        // Arrange
        var name = PatientName.Create("Jan", "Novák");
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Patient.Create(name, futureDate));
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdatePatient()
    {
        // Arrange
        var patient = Patient.Create(
            PatientName.Create("Jan", "Novák"),
            new DateOnly(1980, 1, 1));
        
        var newName = PatientName.Create("Petr", "Svoboda");

        // Act
        patient.UpdateName(newName);

        // Assert
        Assert.Equal(newName, patient.Name);
        Assert.NotNull(patient.UpdatedAt);
    }

    [Fact]
    public void AddDiagnosticResult_WithValidDiagnosis_ShouldAddResult()
    {
        // Arrange
        var patient = Patient.Create(
            PatientName.Create("Jan", "Novák"),
            new DateOnly(1980, 1, 1));
        
        var diagnosis = "Chřipka";
        var notes = "Lehká forma";

        // Act
        patient.AddDiagnosticResult(diagnosis, notes);

        // Assert
        Assert.Single(patient.DiagnosticResults);
        var result = patient.DiagnosticResults.First();
        Assert.Equal(diagnosis, result.Diagnosis.Value);
        Assert.Equal(notes, result.Notes);
        Assert.NotNull(patient.UpdatedAt);
    }

    [Fact]
    public void AddDiagnosticResult_WithEmptyDiagnosis_ShouldThrowException()
    {
        // Arrange
        var patient = Patient.Create(
            PatientName.Create("Jan", "Novák"),
            new DateOnly(1980, 1, 1));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => patient.AddDiagnosticResult("", "notes"));
    }

    [Fact]
    public void GetAge_WithValidDateOfBirth_ShouldCalculateCorrectAge()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dateOfBirth = today.AddYears(-30).AddDays(-1); // 30 years and 1 day ago
        var patient = Patient.Create(
            PatientName.Create("Jan", "Novák"),
            dateOfBirth);

        // Act
        var age = patient.GetAge();

        // Assert
        Assert.Equal(30, age);
    }

    [Fact]
    public void GetLastDiagnosis_WithMultipleResults_ShouldReturnLatest()
    {
        // Arrange
        var patient = Patient.Create(
            PatientName.Create("Jan", "Novák"),
            new DateOnly(1980, 1, 1));
        
        patient.AddDiagnosticResult("Chřipka", "První diagnóza");
        // Small delay to ensure different timestamps
        Thread.Sleep(10);
        patient.AddDiagnosticResult("Angína", "Druhá diagnóza");

        // Act
        var lastDiagnosis = patient.GetLastDiagnosis();

        // Assert
        Assert.Equal("Angína", lastDiagnosis);
    }
}