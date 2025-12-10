using HealthHub.Maui.Models;

namespace HealthHub.Maui.Services;

public interface IPatientService
{
    Task<PaginationResponseDto<PatientDto>> GetPatientsAsync(string? searchTerm = null, int page = 1, int pageSize = 20);
    Task<PatientDetailDto?> GetPatientAsync(Guid id);
    Task<PatientDto> CreatePatientAsync(string firstName, string lastName, DateOnly dateOfBirth);
    Task<PatientDto> UpdatePatientAsync(Guid patientId, string? firstName = null, string? lastName = null, DateOnly? dateOfBirth = null);
    Task<bool> DeletePatientAsync(Guid patientId);
}

public interface IDiagnosticResultService
{
    Task<IEnumerable<DiagnosticResultDto>> GetPatientDiagnosticResultsAsync(Guid patientId, int? limit = null);
    Task<DiagnosticResultDto> AddDiagnosticResultAsync(Guid patientId, string diagnosis, string? notes = null);
    Task<DiagnosticResultDto> UpdateDiagnosticResultAsync(Guid diagnosticResultId, string? diagnosis = null, string? notes = null);
}

public interface ISettingsService
{
    string? GraphQLEndpoint { get; set; }
    bool UseLocalStorage { get; set; }
    Task SaveSettingsAsync();
    Task LoadSettingsAsync();
}