# HealthHub - GraphQL API Documentation

## Overview

HealthHub provides a modern GraphQL API built on the HotChocolate framework. The API implements CQRS pattern with clear separation of read and write operations.

**Base URL**: `/graphql`
**Authentication**: JWT Bearer Token
**Schema**: Code-first with automatic generation

## Authentication

### Obtaining JWT Token

```bash
curl -X POST http://localhost:5023/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"password"}'
```

**Response**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### Using JWT Token

Add the token to GraphQL requests as Authorization header:

```bash
curl -X POST http://localhost:5023/graphql \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{"query":"query { patients { id firstName } }"}'
```

## GraphQL Schema Reference

### Query Operations

#### GetPatients - Patient list with pagination and filtering

```graphql
query GetPatients($searchTerm: String, $page: Int, $pageSize: Int) {
  patients(searchTerm: $searchTerm, page: $page, pageSize: $pageSize) {
    id
    firstName
    lastName
    fullName
    age
    lastDiagnosis
    dateOfBirth
    createdAt
  }
}
```

**Variables**:
```json
{
  "searchTerm": "John",
  "page": 1,
  "pageSize": 10
}
```

**Response**:
```json
{
  "data": {
    "patients": [
      {
        "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "firstName": "John",
        "lastName": "Doe",
        "fullName": "John Doe",
        "age": 42,
        "lastDiagnosis": "Hypertension",
        "dateOfBirth": "1980-01-01",
        "createdAt": "2024-01-15T10:30:00Z"
      }
    ]
  }
}
```

#### GetPatient - Patient details with diagnostic history

```graphql
query GetPatient($id: UUID!) {
  patient(id: $id) {
    id
    firstName
    lastName
    fullName
    age
    dateOfBirth
    lastDiagnosis
    createdAt
    diagnosticResults {
      id
      diagnosis
      notes
      timestampUtc
    }
  }
}
```

**Variables**:
```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

#### GetPatientDiagnosticResults - Patient diagnostic results

```graphql
query GetPatientDiagnosticResults($patientId: UUID!, $limit: Int) {
  patientDiagnosticResults(patientId: $patientId, limit: $limit) {
    id
    diagnosis
    notes
    timestampUtc
  }
}
```

#### GetDiagnoses - Get all diagnoses with filtering and pagination

```graphql
query GetDiagnoses(
  $type: String
  $createdAfter: DateTime
  $createdBefore: DateTime
  $skip: Int
  $take: Int
) {
  diagnoses(
    type: $type
    createdAfter: $createdAfter
    createdBefore: $createdBefore
    skip: $skip
    take: $take
  ) {
    id
    patientId
    diagnosis
    notes
    timestampUtc
    createdAt
  }
}

# Example with fragments
fragment DiagnosisFields on DiagnosticResult {
  id
  patientId
  diagnosis
  notes
  timestampUtc
  createdAt
}

query GetDiagnosesWithFragment($type: String) {
  diagnoses(type: $type) {
    ...DiagnosisFields
  }
}
```

**Variables:**
```json
{
  "type": "Chronic",
  "createdAfter": "2023-01-01T00:00:00Z",
  "skip": 0,
  "take": 10
}
```

**Response:**
```json
{
  "data": {
    "diagnoses": [
      {
        "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "patientId": "b2c3d4e5-f6g7-8901-abcd-ef1234567890",
        "diagnosis": "Hypertension (Chronic)",
        "notes": "Patient requires regular monitoring",
        "timestampUtc": "2023-05-15T10:30:00Z",
        "createdAt": "2023-05-15T10:30:00Z"
      }
    ]
  }
}
```

### Mutation Operations

#### CreatePatient - Create a new patient

```graphql
mutation CreatePatient($command: CreatePatientCommandInput!) {
  createPatient(command: $command) {
    id
    firstName
    lastName
    fullName
    age
    dateOfBirth
  }
}
```

**Variables**:
```json
{
  "command": {
    "firstName": "John",
    "lastName": "Doe",
    "dateOfBirth": "1980-01-01"
  }
}
```

#### UpdatePatient - Update patient

```graphql
mutation UpdatePatient($command: UpdatePatientCommandInput!) {
  updatePatient(command: $command) {
    id
    firstName
    lastName
    fullName
    age
    dateOfBirth
  }
}
```

**Variables**:
```json
{
  "command": {
    "patientId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "firstName": "John",
    "lastName": "Smith"
  }
}
```

#### AddDiagnosticResult - Add diagnostic result

```graphql
mutation AddDiagnosticResult($input: AddDiagnosticResultInput!) {
  addDiagnosticResult(command: $input) {
    id
    patientId
    diagnosis
    notes
    timestampUtc
  }
}
```

**Variables**:
```json
{
  "input": {
    "patientId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "diagnosis": "Diabetes mellitus",
    "notes": "Stable condition, diet recommended"
  }
}
```

#### DeletePatient - Delete patient

```graphql
mutation DeletePatient($input: DeletePatientInput!) {
  deletePatient(command: $input)
}
```

**Variables**:
```json
{
  "input": {
    "patientId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
  }
}
```

## Input Types

### CreatePatientCommandInput
```graphql
input CreatePatientCommandInput {
  firstName: String!
  lastName: String!
  dateOfBirth: Date!
}
```

### UpdatePatientCommandInput
```graphql
input UpdatePatientCommandInput {
  patientId: UUID!
  firstName: String
  lastName: String
  dateOfBirth: Date
}
```

### AddDiagnosticResultInput
```graphql
input AddDiagnosticResultInput {
  patientId: UUID!
  diagnosis: String!
  notes: String
}
```

### DeletePatientInput
```graphql
input DeletePatientInput {
  patientId: UUID!
}
```

## Type Definitions

### Patient Type
```graphql
type Patient {
  id: UUID!
  firstName: String!
  lastName: String!
  fullName: String!
  age: Int!
  dateOfBirth: Date!
  lastDiagnosis: String
  createdAt: DateTime!
  updatedAt: DateTime
  diagnosticResults: [DiagnosticResult!]
}
```

### DiagnosticResult Type
```graphql
type DiagnosticResult {
  id: UUID!
  patientId: UUID!
  diagnosis: String!
  notes: String
  timestampUtc: DateTime!
  createdAt: DateTime!
}
```

## Error Handling

### Standard Error Responses

GraphQL API returns standardized error responses:

```json
{
  "errors": [
    {
      "message": "Patient not found",
      "path": ["patient"],
      "extensions": {
        "code": "PATIENT_NOT_FOUND",
        "timestamp": "2024-01-15T10:30:00Z"
      }
    }
  ],
  "data": null
}
```

### Common Error Codes

| Error Code | Description | Resolution |
|------------|-------------|------------|
| `UNAUTHENTICATED` | Missing or invalid JWT token | Obtain valid token |
| `PATIENT_NOT_FOUND` | Patient with given ID does not exist | Verify patientId |
| `VALIDATION_ERROR` | Invalid input data | Fix input according to schema |
| `DATABASE_ERROR` | Database error | Contact administrator |

## Pagination and Filtering

### Built-in Pagination

HotChocolate provides built-in pagination using `[UsePaging]` attribute:

```graphql
query {
  patients {
    nodes {
      id
      firstName
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
      startCursor
      endCursor
    }
    totalCount
  }
}
```

### Filtering and Sorting

```graphql
query {
  patients(
    where: {
      firstName: { contains: "John" }
      age: { gt: 30 }
    }
    order: { lastName: ASC }
  ) {
    nodes {
      id
      firstName
      lastName
      age
    }
  }
}
```

## Performance Best Practices

### Query Optimization

1. **Select Only Needed Fields** - Minimize transmitted data
2. **Use Variables** - Parameterize queries for caching
3. **Batch Related Queries** - Combine related data

### Example: Optimized Patient Query

```graphql
# Bad - transmits unnecessary data
query {
  patient(id: "123") {
    id
    firstName
    lastName
    diagnosticResults {
      id
      diagnosis
      notes
      timestampUtc
      # Unnecessary data...
    }
  }
}

# Good - only needed data
query GetPatientOverview($id: UUID!) {
  patient(id: $id) {
    id
    firstName
    lastName
    age
    lastDiagnosis
  }
}

query GetPatientDetails($id: UUID!) {
  patient(id: $id) {
    diagnosticResults(limit: 5) {
      diagnosis
      timestampUtc
    }
  }
}
```

## Integration Examples

### Frontend Integration (React + Apollo)

```typescript
import { useQuery, useMutation } from '@apollo/client';
import { GET_PATIENTS, CREATE_PATIENT } from './queries';

function PatientList() {
  const { loading, error, data } = useQuery(GET_PATIENTS);
  const [createPatient] = useMutation(CREATE_PATIENT);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;

  return (
    <div>
      {data.patients.map(patient => (
        <div key={patient.id}>{patient.fullName}</div>
      ))}
    </div>
  );
}
```

### Backend Integration (C#)

```csharp
// GraphQL Query class
public class Query
{
    [Authorize]
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<PatientDto>> GetPatientsAsync(
        [Service] IQueryHandler<GetPatientsQuery, IEnumerable<PatientDto>> handler)
    {
        var query = new GetPatientsQuery();
        return await handler.Handle(query, CancellationToken.None);
    }
}
```

## Testing the API

### GraphQL Playground

In development environment, GraphQL Playground is available at `/graphql`:

1. Open http://localhost:5023/graphql
2. Add Authorization header with JWT token
3. Test queries and mutations

### Example Test Queries

```graphql
# Test authentication
query {
  patients {
    id
    firstName
  }
}

# Test patient creation
mutation {
  createPatient(command: {
    firstName: "Test"
    lastName: "User"
    dateOfBirth: "1990-01-01"
  }) {
    id
    fullName
  }
}
```

## Rate Limiting and Security

### Current Security Measures

- **JWT Authentication** - All mutations require authentication
- **Input Validation** - Domain-level validation in entities
- **CORS Protection** - Strict origin policy

### Future Enhancements

- **Rate Limiting** - API call limits per user
- **Query Depth Limiting** - Prevention of complex queries
- **Persisted Queries** - Whitelist of allowed queries

---

This GraphQL API provides a flexible and efficient way to access patient data with excellent performance and developer experience.