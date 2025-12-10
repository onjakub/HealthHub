using HealthHub.Maui.Models;
using Newtonsoft.Json.Linq;

namespace HealthHub.Maui.Services;

public class PatientService : GraphQLClientService, IPatientService
{
    public PatientService(IGraphQLClient client, IAuthService authService) : base(client, authService)
    {
    }

    public async Task<PaginationResponseDto<PatientDto>> GetPatientsAsync(string? searchTerm = null, int page = 1, int pageSize = 20)
    {
        var query = @"
            query GetPatients($searchTerm: String, $page: Int, $pageSize: Int) {
                patients(searchTerm: $searchTerm, page: $page, pageSize: $pageSize) {
                    nodes {
                        id
                        firstName
                        lastName
                        fullName
                        age
                        lastDiagnosis
                        createdAt
                        dateOfBirth
                    }
                    pageInfo {
                        hasNextPage
                        hasPreviousPage
                        startCursor
                        endCursor
                    }
                    totalCount
                }
            }";

        var variables = new
        {
            searchTerm,
            page,
            pageSize
        };

        var response = await ExecuteQueryAsync<JObject>(query, variables);
        var patientsData = response["patients"];

        return new PaginationResponseDto<PatientDto>
        {
            Nodes = patientsData["nodes"]?.Select(node => new PatientDto
            {
                Id = Guid.Parse(node["id"]?.ToString() ?? Guid.Empty.ToString()),
                FirstName = node["firstName"]?.ToString() ?? string.Empty,
                LastName = node["lastName"]?.ToString() ?? string.Empty,
                FullName = node["fullName"]?.ToString() ?? string.Empty,
                Age = node["age"]?.Value<int>() ?? 0,
                LastDiagnosis = node["lastDiagnosis"]?.ToString(),
                CreatedAt = node["createdAt"]?.Value<DateTime>() ?? DateTime.MinValue,
                DateOfBirth = DateOnly.Parse(node["dateOfBirth"]?.ToString() ?? DateOnly.MinValue.ToString())
            }).ToList() ?? new List<PatientDto>(),
            PageInfo = new PageInfoDto
            {
                HasNextPage = patientsData["pageInfo"]?["hasNextPage"]?.Value<bool>() ?? false,
                HasPreviousPage = patientsData["pageInfo"]?["hasPreviousPage"]?.Value<bool>() ?? false,
                StartCursor = patientsData["pageInfo"]?["startCursor"]?.ToString(),
                EndCursor = patientsData["pageInfo"]?["endCursor"]?.ToString()
            },
            TotalCount = patientsData["totalCount"]?.Value<int>() ?? 0
        };
    }

    public async Task<PatientDetailDto?> GetPatientAsync(Guid id)
    {
        var query = @"
            query GetPatient($id: UUID!) {
                patient(id: $id) {
                    id
                    firstName
                    lastName
                    fullName
                    dateOfBirth
                    age
                    lastDiagnosis
                    createdAt
                    diagnosticResults {
                        id
                        diagnosis
                        notes
                        timestampUtc
                    }
                }
            }";

        var variables = new { id };

        try
        {
            var response = await ExecuteQueryAsync<JObject>(query, variables);
            var patientData = response["patient"];

            if (patientData == null)
                return null;

            return new PatientDetailDto
            {
                Id = Guid.Parse(patientData["id"]?.ToString() ?? Guid.Empty.ToString()),
                FirstName = patientData["firstName"]?.ToString() ?? string.Empty,
                LastName = patientData["lastName"]?.ToString() ?? string.Empty,
                FullName = patientData["fullName"]?.ToString() ?? string.Empty,
                Age = patientData["age"]?.Value<int>() ?? 0,
                LastDiagnosis = patientData["lastDiagnosis"]?.ToString(),
                CreatedAt = patientData["createdAt"]?.Value<DateTime>() ?? DateTime.MinValue,
                DateOfBirth = DateOnly.Parse(patientData["dateOfBirth"]?.ToString() ?? DateOnly.MinValue.ToString()),
                DiagnosticResults = patientData["diagnosticResults"]?.Select(dr => new DiagnosticResultDto
                {
                    Id = Guid.Parse(dr["id"]?.ToString() ?? Guid.Empty.ToString()),
                    PatientId = Guid.Parse(dr["patientId"]?.ToString() ?? Guid.Empty.ToString()),
                    Diagnosis = dr["diagnosis"]?.ToString() ?? string.Empty,
                    Notes = dr["notes"]?.ToString(),
                    TimestampUtc = dr["timestampUtc"]?.Value<DateTime>() ?? DateTime.MinValue
                }).ToList() ?? new List<DiagnosticResultDto>()
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<PatientDto> CreatePatientAsync(string firstName, string lastName, DateOnly dateOfBirth)
    {
        var mutation = @"
            mutation CreatePatient($command: CreatePatientCommandInput!) {
                createPatient(command: $command) {
                    id
                    firstName
                    lastName
                    fullName
                    dateOfBirth
                    age
                }
            }";

        var variables = new
        {
            command = new
            {
                firstName,
                lastName,
                dateOfBirth = dateOfBirth.ToString("yyyy-MM-dd")
            }
        };

        var response = await ExecuteMutationAsync<JObject>(mutation, variables);
        var patientData = response["createPatient"];

        return new PatientDto
        {
            Id = Guid.Parse(patientData["id"]?.ToString() ?? Guid.Empty.ToString()),
            FirstName = patientData["firstName"]?.ToString() ?? string.Empty,
            LastName = patientData["lastName"]?.ToString() ?? string.Empty,
            FullName = patientData["fullName"]?.ToString() ?? string.Empty,
            Age = patientData["age"]?.Value<int>() ?? 0,
            DateOfBirth = DateOnly.Parse(patientData["dateOfBirth"]?.ToString() ?? DateOnly.MinValue.ToString())
        };
    }

    public async Task<PatientDto> UpdatePatientAsync(Guid patientId, string? firstName = null, string? lastName = null, DateOnly? dateOfBirth = null)
    {
        var mutation = @"
            mutation UpdatePatient($command: UpdatePatientCommandInput!) {
                updatePatient(command: $command) {
                    id
                    firstName
                    lastName
                    fullName
                    dateOfBirth
                    age
                }
            }";

        var variables = new
        {
            command = new
            {
                patientId,
                firstName,
                lastName,
                dateOfBirth = dateOfBirth?.ToString("yyyy-MM-dd")
            }
        };

        var response = await ExecuteMutationAsync<JObject>(mutation, variables);
        var patientData = response["updatePatient"];

        return new PatientDto
        {
            Id = Guid.Parse(patientData["id"]?.ToString() ?? Guid.Empty.ToString()),
            FirstName = patientData["firstName"]?.ToString() ?? string.Empty,
            LastName = patientData["lastName"]?.ToString() ?? string.Empty,
            FullName = patientData["fullName"]?.ToString() ?? string.Empty,
            Age = patientData["age"]?.Value<int>() ?? 0,
            DateOfBirth = DateOnly.Parse(patientData["dateOfBirth"]?.ToString() ?? DateOnly.MinValue.ToString())
        };
    }

    public async Task<bool> DeletePatientAsync(Guid patientId)
    {
        var mutation = @"
            mutation DeletePatient($command: DeletePatientInput!) {
                deletePatient(command: $command)
            }";

        var variables = new
        {
            command = new { patientId }
        };

        try
        {
            var response = await ExecuteMutationAsync<JObject>(mutation, variables);
            return response["deletePatient"]?.Value<bool>() ?? false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}