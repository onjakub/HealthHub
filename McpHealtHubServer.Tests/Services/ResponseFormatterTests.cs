using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McpHealtHubServer.Models;
using McpHealtHubServer.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace McpHealtHubServer.Tests.Services;

public class ResponseFormatterTests
{
    private readonly Mock<ILogger<ResponseFormatter>> _loggerMock;
    private readonly ResponseFormatter _formatter;

    public ResponseFormatterTests()
    {
        _loggerMock = new Mock<ILogger<ResponseFormatter>>();
        _formatter = new ResponseFormatter(_loggerMock.Object);
    }

    [Fact]
    public void FormatPatientList_WithEmptyList_ReturnsNoPatientsMessage()
    {
        // Arrange
        var patients = new List<PatientDto>();

        // Act
        var result = _formatter.FormatPatientList(patients);

        // Assert
        Assert.Equal("No patients found.", result);
    }

    [Fact]
    public void FormatPatientList_WithPatients_ReturnsFormattedString()
    {
        // Arrange
        var patients = new List<PatientDto>
        {
            new PatientDto
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                FullName = "John Doe",
                DateOfBirth = new DateOnly(1990, 1, 1),
                Age = 34,
                LastDiagnosis = "Healthy",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }
        };

        // Act
        var result = _formatter.FormatPatientList(patients);

        // Assert
        Assert.Contains("Found 1 patients:", result);
        Assert.Contains("John Doe", result);
        Assert.Contains("Age: 34", result);
    }

    [Fact]
    public void FormatPatientDetail_WithNullPatient_ReturnsNotFoundMessage()
    {
        // Act
        var result = _formatter.FormatPatientDetail(null!);

        // Assert
        Assert.Equal("Patient not found.", result);
    }

    [Fact]
    public void FormatPatientDetail_WithPatient_ReturnsFormattedString()
    {
        // Arrange
        var patient = new PatientDetailDto
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Smith",
            FullName = "Jane Smith",
            DateOfBirth = new DateOnly(1985, 5, 15),
            Age = 39,
            LastDiagnosis = "Checkup required",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            DiagnosticResults = new List<DiagnosticResultDto>
            {
                new DiagnosticResultDto
                {
                    Id = Guid.NewGuid(),
                    Diagnosis = "Routine checkup",
                    Notes = "All normal",
                    TimestampUtc = DateTime.UtcNow
                }
            }
        };

        // Act
        var result = _formatter.FormatPatientDetail(patient);

        // Assert
        Assert.Contains("Patient Details:", result);
        Assert.Contains("Jane Smith", result);
        Assert.Contains("Age: 39", result);
        Assert.Contains("Routine checkup", result);
    }

    [Fact]
    public void FormatDiagnosticResults_WithEmptyList_ReturnsNoResultsMessage()
    {
        // Arrange
        var results = new List<DiagnosticResultDto>();

        // Act
        var result = _formatter.FormatDiagnosticResults(results);

        // Assert
        Assert.Equal("No diagnostic results found for this patient.", result);
    }

    [Fact]
    public void FormatDiagnosticResults_WithResults_ReturnsFormattedString()
    {
        // Arrange
        var results = new List<DiagnosticResultDto>
        {
            new DiagnosticResultDto
            {
                Id = Guid.NewGuid(),
                Diagnosis = "Blood test",
                Notes = "Cholesterol elevated",
                TimestampUtc = DateTime.UtcNow
            }
        };

        // Act
        var result = _formatter.FormatDiagnosticResults(results);

        // Assert
        Assert.Contains("Found 1 diagnostic results:", result);
        Assert.Contains("Blood test", result);
        Assert.Contains("Cholesterol elevated", result);
    }

    [Fact]
    public void FormatPatientSearch_WithEmptyList_ReturnsNoResultsMessage()
    {
        // Arrange
        var patients = new List<PatientDto>();

        // Act
        var result = _formatter.FormatPatientSearch(patients, "test", null, null, null);

        // Assert
        Assert.Equal("No patients found matching the search criteria.", result);
    }

    [Fact]
    public void FormatPatientSearch_WithPatients_ReturnsFormattedString()
    {
        // Arrange
        var patients = new List<PatientDto>
        {
            new PatientDto
            {
                Id = Guid.NewGuid(),
                FirstName = "Bob",
                LastName = "Johnson",
                FullName = "Bob Johnson",
                DateOfBirth = new DateOnly(1975, 10, 20),
                Age = 48,
                LastDiagnosis = "Annual checkup",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }
        };

        // Act
        var result = _formatter.FormatPatientSearch(patients, "Johnson", 40, 50, true);

        // Assert
        Assert.Contains("Search Results:", result);
        Assert.Contains("Search term: Johnson", result);
        Assert.Contains("Minimum age: 40", result);
        Assert.Contains("Maximum age: 50", result);
        Assert.Contains("Has recent diagnosis: True", result);
        Assert.Contains("Bob Johnson", result);
    }

    [Fact]
    public void FormatError_ReturnsFormattedErrorString()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var result = _formatter.FormatError(message);

        // Assert
        Assert.Equal("Error: Test error message", result);
    }

    [Fact]
    public void FormatSuccess_ReturnsFormattedSuccessString()
    {
        // Arrange
        var message = "Test success message";

        // Act
        var result = _formatter.FormatSuccess(message);

        // Assert
        Assert.Equal("Success: Test success message", result);
    }
}