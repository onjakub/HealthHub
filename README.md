# HealthHub - Full Stack Patient Management Application

Aplikace pro správu a zobrazování dat pacientů s GraphQL API a jednoduchým uživatelským rozhraním. Implementuje Pragmatic Clean Architecture s DDD + CQRS patternem.

## Architektura

Aplikace je postavena jako modulární monolit s následujícími vrstvami:

### Domain Layer (Core Business Logic)
- **Entities**: `Patient`, `DiagnosticResult` s domain logikou
- **Value Objects**: `PatientName`, `Diagnosis` pro validaci dat
- **Repository Interfaces**: `IPatientRepository`, `IDiagnosticResultRepository`

### Application Layer (Use Cases)
- **Commands**: `CreatePatientCommand`, `UpdateDiagnosisCommand` atd.
- **Queries**: `GetPatientsQuery`, `GetPatientDetailsQuery` atd.
- **Handlers**: Command a Query handlery implementující CQRS pattern
- **DTOs**: Data transfer objects pro komunikaci mezi vrstvami

### Infrastructure Layer (External Concerns)
- **Data Access**: EF Core s PostgreSQL
- **Repositories**: Implementace repository interfaces
- **Authentication**: JWT-based authentication

### Presentation Layer (API & Frontend)
- **GraphQL API**: HotChocolate GraphQL server
- **Frontend**: Jednoduché HTML/CSS/JavaScript rozhraní

## Funkce

### Backend (GraphQL API)
- **Queries**:
  - `getPatients`: Seznam pacientů s filtrováním a stránkováním
  - `getPatient`: Detail pacienta s diagnostickou historií
  - `getPatientDiagnosticResults`: Diagnostické výsledky pacienta

- **Mutations**:
  - `createPatient`: Vytvoření nového pacienta
  - `updatePatient`: Aktualizace údajů pacienta
  - `addDiagnosticResult`: Přidání diagnostického výsledku
  - `updateDiagnosticResult`: Aktualizace diagnostického výsledku
  - `deletePatient`: Smazání pacienta

### Frontend
- Přihlášení pomocí JWT tokenu
- Zobrazení seznamu pacientů s vyhledáváním
- Detailní zobrazení pacienta s diagnostickou historií
- Formulář pro přidání nového pacienta
- Responzivní design

## Technologie

### Backend
- **.NET 10.0** s HotChocolate GraphQL
- **Entity Framework Core** s PostgreSQL
- **JWT Authentication**
- **Docker** pro kontejnerizaci

### Frontend
- **HTML5**, **CSS3**, **JavaScript (ES6+)**
- **GraphQL Client** s fetch API
- Responzivní design s CSS Grid/Flexbox

## Instalace a spuštění

### Prerekvizity
- Docker a Docker Compose
- .NET 10.0 SDK (pro lokální vývoj)

### Spuštění s Docker Compose

1. Naklonujte repository:
```bash
git clone <repository-url>
cd HealthHub
```

2. Spusťte aplikaci:
```bash
docker-compose up -d
```

3. Aplikace bude dostupná na:
   - Frontend: http://localhost:8080
   - GraphQL API: http://localhost:8080/graphql
   - OpenAPI: http://localhost:8080/openapi (pouze v development)

### Lokální vývoj

1. Restore dependencies:
```bash
cd HealthHub
dotnet restore
```

2. Spusťte databázi:
```bash
docker-compose up healthhub-db -d
```

3. Spusťte aplikaci:
```bash
dotnet run
```

## Konfigurace

### Environment Variables
- `DB_CONNECTION`: PostgreSQL connection string
- `JWT_KEY`: Secret key pro JWT tokeny
- `ASPNETCORE_ENVIRONMENT`: Environment (Development/Production)

### Default Configuration
Výchozí konfigurace je v `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=healthhub;Username=health;Password=healthpwd"
  },
  "Jwt": {
    "Key": "dev-secret-change-me-please-very-long-secure-key",
    "Issuer": "HealthHub",
    "Audience": "HealthHubAudience",
    "ExpiryHours": 12
  }
}
```

## Použití

### Autentizace
1. Otevřete aplikaci na http://localhost:8080
2. Přihlaste se s libovolným uživatelským jménem a heslem
3. Token je automaticky uložen v localStorage

### GraphQL Queries

**Získání seznamu pacientů:**
```graphql
query {
  getPatients {
    id
    firstName
    lastName
    fullName
    age
    lastDiagnosis
    createdAt
  }
}
```

**Vytvoření pacienta:**
```graphql
mutation {
  createPatient(command: {
    firstName: "Jan"
    lastName: "Novák"
    dateOfBirth: "1980-01-01"
  }) {
    id
    fullName
    age
  }
}
```

## Testování

Pro spuštění testů:
```bash
cd HealthHub
dotnet test
```

## Docker

### Build image:
```bash
docker build -t healthhub -f HealthHub/Dockerfile .
```

### Spuštění s PostgreSQL:
```bash
docker-compose up -d
```

## Bezpečnost

- JWT-based authentication
- Input validation na všech úrovních
- Secure configuration management
- Health checks pro monitoring

## Vývoj

### Project Structure
```
HealthHub/
├── Domain/           # Domain layer
├── Application/      # Application layer (CQRS)
├── Infrastructure/   # Infrastructure layer
├── Presentation/     # Presentation layer (GraphQL + Frontend)
├── Tests/           # Unit a integration tests
└── wwwroot/         # Static files (frontend)
```

### Code Conventions
- Clean Architecture principles
- Domain-Driven Design
- CQRS pattern
- Dependency Injection
- Async/await pattern

## License

Tento projekt je vytvořen pro demonstrační účely.