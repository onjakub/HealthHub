using HealthHub.Application.Commands;
using HealthHub.Application.Handlers;
using HealthHub.Application.Queries;
using HealthHub.Domain.Entities;
using HealthHub.Domain.Interfaces;
using HealthHub.Domain.ValueObjects;
using Moq;
using Xunit;

namespace Application.Tests;

public class CreatePatientCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreatePatient()
    {
        // Arrange
        var mockRepository = new Mock<IPatientRepository>();
        var handler = new CreatePatientCommandHandler(mockRepository.Object);
        
        var dateOfBirth = new DateOnly(1980, 1, 1);
        var command = new CreatePatientCommand
        {
            FirstName = "Jan",
            LastName = "Novák",
            DateOfBirth = dateOfBirth
        };

        var createdPatient = Patient.Create(
            PatientName.Create("Jan", "Novák"),
            dateOfBirth);

        mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPatient);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jan", result.FirstName);
        Assert.Equal("Novák", result.LastName);
        Assert.Equal("Jan Novák", result.FullName);
        Assert.Equal(dateOfBirth, result.DateOfBirth);
        Assert.True(result.Age > 0); // Age should be positive
        mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidDateOfBirth_ShouldThrowException()
    {
        // Arrange
        var mockRepository = new Mock<IPatientRepository>();
        var handler = new CreatePatientCommandHandler(mockRepository.Object);
        
        var command = new CreatePatientCommand
        {
            FirstName = "Jan",
            LastName = "Novák",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) // Future date
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            handler.Handle(command, CancellationToken.None));
        
        mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class GetPatientsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithNoFilters_ShouldReturnAllPatients()
    {
        // Arrange
        var mockRepository = new Mock<IPatientRepository>();
        var handler = new GetPatientsQueryHandler(mockRepository.Object);
        
        var patients = new List<Patient>
        {
            Patient.Create(PatientName.Create("Jan", "Novák"), new DateOnly(1980, 1, 1)),
            Patient.Create(PatientName.Create("Petr", "Svoboda"), new DateOnly(1990, 1, 1))
        };

        mockRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(patients);

        var query = new GetPatientsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        mockRepository.Verify(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldFilterPatients()
    {
        // Arrange
        var mockRepository = new Mock<IPatientRepository>();
        var handler = new GetPatientsQueryHandler(mockRepository.Object);
        
        var patients = new List<Patient>
        {
            Patient.Create(PatientName.Create("Jan", "Novák"), new DateOnly(1980, 1, 1)),
            Patient.Create(PatientName.Create("Petr", "Svoboda"), new DateOnly(1990, 1, 1))
        };

        mockRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(patients);

        var query = new GetPatientsQuery { SearchTerm = "Jan" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Jan", result.First().FirstName);
        mockRepository.Verify(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldApplyPagination()
    {
        // Arrange
        var mockRepository = new Mock<IPatientRepository>();
        var handler = new GetPatientsQueryHandler(mockRepository.Object);
        
        var patients = new List<Patient>
        {
            Patient.Create(PatientName.Create("Jan", "Novák"), new DateOnly(1980, 1, 1)),
            Patient.Create(PatientName.Create("Petr", "Svoboda"), new DateOnly(1990, 1, 1)),
            Patient.Create(PatientName.Create("Anna", "Kovářová"), new DateOnly(2000, 1, 1))
        };

        mockRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(patients);

        var query = new GetPatientsQuery { Page = 2, PageSize = 1 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Petr", result.First().FirstName); // Second page, first item
        mockRepository.Verify(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class GetPatientByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithValidId_ShouldReturnPatientWithDiagnosticResults()
    {
        // Arrange
        var mockPatientRepository = new Mock<IPatientRepository>();
        var mockDiagnosticRepository = new Mock<IDiagnosticResultRepository>();
        var handler = new GetPatientByIdQueryHandler(mockPatientRepository.Object, mockDiagnosticRepository.Object);
        
        var patientId = Guid.NewGuid();
        var patient = Patient.Create(PatientName.Create("Jan", "Novák"), new DateOnly(1980, 1, 1));
        
        var diagnosticResults = new List<HealthHub.Domain.Entities.DiagnosticResult>
        {
            HealthHub.Domain.Entities.DiagnosticResult.Create(patientId, "Chřipka", "Lehká forma")
        };

        mockPatientRepository.Setup(repo => repo.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        mockDiagnosticRepository.Setup(repo => repo.GetByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(diagnosticResults);

        var query = new GetPatientByIdQuery { PatientId = patientId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jan", result!.FirstName);
        Assert.Single(result.DiagnosticResults);
        Assert.Equal("Chřipka", result.DiagnosticResults.First().Diagnosis);
        
        mockPatientRepository.Verify(repo => repo.GetByIdAsync(patientId, It.IsAny<CancellationToken>()), Times.Once);
        mockDiagnosticRepository.Verify(repo => repo.GetByPatientIdAsync(patientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var mockPatientRepository = new Mock<IPatientRepository>();
        var mockDiagnosticRepository = new Mock<IDiagnosticResultRepository>();
        var handler = new GetPatientByIdQueryHandler(mockPatientRepository.Object, mockDiagnosticRepository.Object);
        
        var patientId = Guid.NewGuid();

        mockPatientRepository.Setup(repo => repo.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        var query = new GetPatientByIdQuery { PatientId = patientId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
        mockPatientRepository.Verify(repo => repo.GetByIdAsync(patientId, It.IsAny<CancellationToken>()), Times.Once);
        mockDiagnosticRepository.Verify(repo => repo.GetByPatientIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}