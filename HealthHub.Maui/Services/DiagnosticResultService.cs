using HealthHub.Maui.Models;
using Newtonsoft.Json.Linq;

namespace HealthHub.Maui.Services;

public class DiagnosticResultService : GraphQLClientService, IDiagnosticResultService
{
    public DiagnosticResultService(HttpClient httpClient, IAuthService authService) : base(httpClient, authService)
    {
    }

    public async Task<IEnumerable<DiagnosticResultDto>> GetPatientDiagnosticResultsAsync(Guid patientId, int? limit = null)
    {
        var query = @"
            query GetPatientDiagnosticResults($patientId: UUID!, $limit: Int) {
                patientDiagnosticResults(patientId: $patientId, limit: $limit) {
                    id
                    diagnosis
                    notes
                    timestampUtc
                }
            }";

        var variables = new { patientId, limit };

        try
        {
            var response = await ExecuteQueryAsync<JObject>(query, variables);
            var results = response["patientDiagnosticResults"];

            return results?.Select(result => new DiagnosticResultDto
            {
                Id = Guid.Parse(result["id"]?.ToString() ?? Guid.Empty.ToString()),
                PatientId = patientId,
                Diagnosis = result["diagnosis"]?.ToString() ?? string.Empty,
                Notes = result["notes"]?.ToString(),
                TimestampUtc = result["timestampUtc"]?.Value<DateTime>() ?? DateTime.MinValue
            }).ToList() ?? new List<DiagnosticResultDto>();
        }
        catch (Exception)
        {
            return new List<DiagnosticResultDto>();
        }
    }

    public async Task<DiagnosticResultDto> AddDiagnosticResultAsync(Guid patientId, string diagnosis, string? notes = null)
    {
        var mutation = @"
            mutation AddDiagnosticResult($command: AddDiagnosticResultCommandInput!) {
                addDiagnosticResult(command: $command) {
                    id
                    patientId
                    diagnosis
                    notes
                    timestampUtc
                }
            }";

        var variables = new
        {
            command = new
            {
                patientId,
                diagnosis,
                notes
            }
        };

        var response = await ExecuteMutationAsync<JObject>(mutation, variables);
        var resultData = response["addDiagnosticResult"];

        return new DiagnosticResultDto
        {
            Id = Guid.Parse(resultData["id"]?.ToString() ?? Guid.Empty.ToString()),
            PatientId = Guid.Parse(resultData["patientId"]?.ToString() ?? Guid.Empty.ToString()),
            Diagnosis = resultData["diagnosis"]?.ToString() ?? string.Empty,
            Notes = resultData["notes"]?.ToString(),
            TimestampUtc = resultData["timestampUtc"]?.Value<DateTime>() ?? DateTime.MinValue
        };
    }

    public async Task<DiagnosticResultDto> UpdateDiagnosticResultAsync(Guid diagnosticResultId, string? diagnosis = null, string? notes = null)
    {
        var mutation = @"
            mutation UpdateDiagnosticResult($command: UpdateDiagnosticResultCommandInput!) {
                updateDiagnosticResult(command: $command) {
                    id
                    patientId
                    diagnosis
                    notes
                    timestampUtc
                }
            }";

        var variables = new
        {
            command = new
            {
                diagnosticResultId,
                diagnosis,
                notes
            }
        };

        var response = await ExecuteMutationAsync<JObject>(mutation, variables);
        var resultData = response["updateDiagnosticResult"];

        return new DiagnosticResultDto
        {
            Id = Guid.Parse(resultData["id"]?.ToString() ?? Guid.Empty.ToString()),
            PatientId = Guid.Parse(resultData["patientId"]?.ToString() ?? Guid.Empty.ToString()),
            Diagnosis = resultData["diagnosis"]?.ToString() ?? string.Empty,
            Notes = resultData["notes"]?.ToString(),
            TimestampUtc = resultData["timestampUtc"]?.Value<DateTime>() ?? DateTime.MinValue
        };
    }
}