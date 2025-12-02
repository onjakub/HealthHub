# HealthHub - Development Guide

## Overview

This document describes the development workflow, contribution guidelines, and troubleshooting common issues when developing the HealthHub application.

## Development Environment Setup

### Prerequisites

```bash
# Verify installed versions
dotnet --version    # .NET 10.0+
node --version      # Node.js 18+
npm --version       # npm 9+
docker --version    # Docker 20+
docker-compose --version
```

### Initial Setup

```bash
# Clone repository
git clone <repository-url>
cd HealthHub

# Install backend dependencies
cd HealthHub
dotnet restore

# Install frontend dependencies
cd ../Frontend
npm install

# Start development environment
docker-compose up -d
```

## Development Workflow

### Backend Development

#### Backend Structure

```
HealthHub/
├── Application/          # CQRS handlers
│   ├── Commands/        # Command objects
│   ├── Queries/         # Query objects
│   ├── Handlers/        # Command/Query handlers
│   └── DTOs/            # Data Transfer Objects
├── Domain/              # Business logic
│   ├── Entities/        # Domain entities
│   ├── ValueObjects/    # Value objects
│   ├── Interfaces/      # Repository interfaces
│   └── Services/        # Domain services
├── Infrastructure/      # External concerns
│   ├── Data/            # EF Core context
│   └── Repositories/    # Repository implementations
└── Presentation/        # API layer
    └── GraphQL/         # GraphQL resolvers
```

#### Creating New Feature

1. **Define domain model** (Domain/Entities/)
2. **Create repository interface** (Domain/Interfaces/)
3. **Implement repository** (Infrastructure/Repositories/)
4. **Create Command/Query** (Application/Commands|Queries/)
5. **Implement handler** (Application/Handlers/)
6. **Add GraphQL resolver** (Presentation/GraphQL/)
7. **Testing** (Tests/)

#### Example: Adding New Feature

```csharp
// 1. Domain Entity
public class MedicalRecord : Entity
{
    public Guid PatientId { get; private set; }
    public string Content { get; private set; }
    // ... business logic
}

// 2. Repository Interface
public interface IMedicalRecordRepository
{
    Task<MedicalRecord?> GetByIdAsync(Guid id);
    Task<MedicalRecord> AddAsync(MedicalRecord record);
}

// 3. Command
public record CreateMedicalRecordCommand : ICommand<MedicalRecordDto>
{
    public Guid PatientId { get; init; }
    public string Content { get; init; } = string.Empty;
}

// 4. Handler
public class CreateMedicalRecordCommandHandler
    : ICommandHandler<CreateMedicalRecordCommand, MedicalRecordDto>
{
    public async Task<MedicalRecordDto> Handle(CreateMedicalRecordCommand command)
    {
        // Implementation
    }
}
```

### Frontend Development

#### Frontend Structure

```
Frontend/
├── app/                 # Next.js App Router
│   ├── layout.tsx       # Root layout
│   ├── page.tsx         # Home page
│   └── globals.css      # Global styles
├── components/          # React components
│   ├── patients/        # Patient-related components
│   ├── auth/            # Authentication components
│   └── shared/          # Shared components
├── lib/                 # Utility functions
│   ├── graphql-client.ts # Apollo Client configuration
│   ├── queries.ts       # GraphQL queries
│   └── api-base.ts      # API base URL helper
└── public/              # Static assets
```

#### Creating New Component

1. **Define TypeScript interface**
2. **Create GraphQL query/mutation**
3. **Implement React component**
4. **Add to routing**
5. **Testing**

#### Example: New Component

```typescript
// 1. TypeScript interface
interface MedicalRecord {
  id: string;
  patientId: string;
  content: string;
  createdAt: string;
}

// 2. GraphQL query
export const GET_MEDICAL_RECORDS = gql`
  query GetMedicalRecords($patientId: UUID!) {
    medicalRecords(patientId: $patientId) {
      id
      content
      createdAt
    }
  }
`;

// 3. React component
export default function MedicalRecordsList({ patientId }: { patientId: string }) {
  const { loading, error, data } = useQuery(GET_MEDICAL_RECORDS, {
    variables: { patientId }
  });

  // Component implementation
}
```

## Testing Strategy

### Backend Testing

#### Unit Tests

```csharp
// Example unit test
public class PatientTests
{
    [Fact]
    public void CreatePatient_WithValidData_ShouldSucceed()
    {
        // Arrange
        var name = PatientName.Create("John", "Doe");
        var dob = new DateOnly(1980, 1, 1);

        // Act
        var patient = Patient.Create(name, dob);

        // Assert
        Assert.NotNull(patient);
        Assert.Equal("John Doe", patient.Name.FullName);
    }
}
```

#### Integration Tests

```csharp
public class PatientIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PatientIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPatients_ShouldReturnPatients()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/graphql?query={ patients { id firstName } }");

        // Assert
        response.EnsureSuccessStatusCode();
    }
}
```

### Frontend Testing

#### Unit Tests

```typescript
// Component test
import { render, screen } from '@testing-library/react';
import PatientList from '../components/patients/PatientList';

test('renders patient list', () => {
  render(<PatientList onEditPatient={() => {}} />);
  expect(screen.getByText('Loading patients...')).toBeInTheDocument();
});
```

#### Integration Tests

```typescript
// GraphQL integration test
import { MockedProvider } from '@apollo/client/testing';
import { GET_PATIENTS } from '../lib/queries';

const mocks = [
  {
    request: {
      query: GET_PATIENTS,
    },
    result: {
      data: {
        patients: {
          nodes: [
            { id: '1', firstName: 'John', lastName: 'Doe' }
          ]
        }
      },
    },
  },
];

test('loads and displays patients', async () => {
  render(
    <MockedProvider mocks={mocks} addTypename={false}>
      <PatientList onEditPatient={() => {}} />
    </MockedProvider>
  );
  
  await waitFor(() => {
    expect(screen.getByText('John Doe')).toBeInTheDocument();
  });
});
```

## Code Quality and Standards

### Backend Standards

#### Coding Conventions

- **C# Conventions** - Follow Microsoft C# coding conventions
- **Async/Await** - Use async/await pattern consistently
- **Dependency Injection** - Use constructor injection
- **Error Handling** - Use exceptions for exceptional cases

#### Architecture Rules

- **Layer Separation** - No cross-layer dependencies
- **Domain Focus** - Business logic in domain layer
- **CQRS Pattern** - Separate commands and queries
- **Repository Pattern** - Abstract data access

### Frontend Standards

#### Coding Conventions

- **TypeScript Strict** - Enable strict type checking
- **Functional Components** - Use React functional components
- **Hooks Pattern** - Use React hooks appropriately
- **CSS Methodology** - Use Tailwind CSS utility classes

#### Architecture Rules

- **Component Separation** - Single responsibility components
- **State Management** - Local state vs. global state
- **API Abstraction** - GraphQL client abstraction
- **Error Boundaries** - Global error handling

## Debugging and Troubleshooting

### Common Backend Issues

#### Database Connection Issues

```bash
# Test database connection
dotnet ef database update
# Check connection string
echo $DB_CONNECTION
```

#### GraphQL Schema Issues

```bash
# Generate GraphQL schema
dotnet run --urls="http://localhost:5023/graphql"
# Check schema in browser
```

#### Dependency Issues

```bash
# Clean restore
dotnet clean
dotnet restore
rm -rf bin/ obj/
```

### Common Frontend Issues

#### Build Issues

```bash
# Clean build
npm run clean:all
npm install
npm run build

# Type checking
npm run type-check

# Linting
npm run lint
```

#### GraphQL Client Issues

```typescript
// Debug GraphQL requests
const client = new ApolloClient({
  link: from([authLink, httpLink]),
  cache: new InMemoryCache(),
  connectToDevTools: true, // Enable Apollo DevTools
});
```

#### Hot Reload Issues

```bash
# Restart dev server
npm run dev
# Check port conflicts
lsof -ti:3000 | xargs kill -9
```

### Debugging Techniques

#### Backend Debugging

```csharp
// Add logging
logger.LogInformation("Processing patient {PatientId}", patientId);

// Use debugger
#if DEBUG
    System.Diagnostics.Debugger.Break();
#endif
```

#### Frontend Debugging

```typescript
// React DevTools
// Install React DevTools browser extension

// Apollo Client DevTools
// Install Apollo Client DevTools

// Console debugging
console.log('Component props:', props);
```

## Performance Optimization

### Backend Optimization

#### Database Optimization

```csharp
// Use AsNoTracking for read operations
return await db.Patients
    .AsNoTracking()
    .ToListAsync();

// Use selective loading
return await db.Patients
    .Select(p => new PatientDto { Id = p.Id, Name = p.Name })
    .ToListAsync();

// Use eager loading to avoid N+1 queries
return await db.DiagnosticResults
    .Include(d => d.Patient)  // Eager load related patient data
    .AsNoTracking()
    .ToListAsync();
```

#### GraphQL Optimization

```csharp
// Use projections
[UseProjection]
public IQueryable<Patient> GetPatients([Service] HealthHubDbContext db)
    => db.Patients;
```

### Frontend Optimization

#### Bundle Optimization

```typescript
// Code splitting
import { lazy } from 'react';
const PatientForm = lazy(() => import('./components/patients/PatientForm'));

// GraphQL query optimization
const GET_PATIENTS = gql`
  query GetPatients {
    patients {
      id
      firstName
      lastName
      # Only needed fields
    }
  }
`;
```

#### Caching Strategy

```typescript
// Apollo Client cache configuration
const client = new ApolloClient({
  cache: new InMemoryCache({
    typePolicies: {
      Patient: {
        keyFields: ["id"],
      },
    },
  }),
});
```

## Contribution Guidelines

### Pull Request Process

1. **Fork repository** - Create personal fork
2. **Create feature branch** - `git checkout -b feature/amazing-feature`
3. **Commit changes** - `git commit -m 'Add amazing feature'`
4. **Push to branch** - `git push origin feature/amazing-feature`
5. **Open Pull Request** - Create PR with description

### Code Review Checklist

- [ ] Code follows project conventions
- [ ] Tests are included and passing
- [ ] Documentation is updated
- [ ] No breaking changes
- [ ] Performance considerations addressed

### Commit Message Convention

```
feat: add patient search functionality
fix: resolve authentication token expiry
docs: update API documentation
style: format code according to guidelines
refactor: improve patient service structure
test: add unit tests for patient validation
chore: update dependencies
```

## Continuous Integration

### GitHub Actions Example

```yaml
name: HealthHub CI

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: '10.0.x'
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore
      
    - name: Test
      run: dotnet test --no-build --verbosity normal
```

---

This development guide provides a complete overview for developers working on the HealthHub project and ensures consistent code quality across the team.