# HealthHub - Full Stack Patient Management System

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![React](https://img.shields.io/badge/React-18-61DAFB?logo=react)
![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript)
![GraphQL](https://img.shields.io/badge/GraphQL-HotChocolate-E10098?logo=graphql)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql)

A comprehensive full-stack patient management application with modern architecture, GraphQL API, and TypeScript React frontend. The application implements Pragmatic Clean Architecture with DDD and CQRS patterns.

## 🚀 Quick Start

### Prerequisites
- **.NET 10.0 SDK**
- **Node.js 18+** and npm
- **Docker** and Docker Compose (optional)
- **PostgreSQL** (locally or in Docker)

### Running with Docker Compose (recommended)

```bash
# Clone the repository
git clone <repository-url>
cd HealthHub

# Start the entire application
docker-compose up -d
```

The application will be available at:
- **Frontend**: http://localhost:8080
- **GraphQL API**: http://localhost:8080/graphql
- **Health check**: http://localhost:8080/health

### Local Development

#### 1. Start the database
```bash
docker-compose up healthhub-db -d
```

#### 2. Start the backend
```bash
cd HealthHub
dotnet restore
dotnet run
```
Backend runs on: **http://localhost:5023**

#### 3. Start the frontend (development mode)
```bash
cd Frontend
npm install
npm run dev
```
Frontend runs on: **http://localhost:3000**

### Production Build
```bash
# Build the frontend
cd Frontend
npm run build

# Run the backend with integrated frontend
cd ..
dotnet run
```

## 📋 Features

### ✅ Implemented Features
- **Patient Management** - CRUD operations for patients
- **Diagnostic Results** - Adding and managing diagnoses
- **GraphQL API** - Modern API with filtering and sorting
- **JWT Authentication** - Secure login
- **Responsive Design** - Optimized for mobile devices
- **Hot Reload** - Fast development with live updates

### 🔄 GraphQL Operations
- **Queries**: `patients`, `patient`, `patientDiagnosticResults`
- **Mutations**: `createPatient`, `updatePatient`, `addDiagnosticResult`, `deletePatient`

## 🏗️ Architecture

The application is built as a modular monolith with the following layers:

### Domain Layer (Core Business Logic)
- **Entities**: `Patient`, `DiagnosticResult` with domain logic
- **Value Objects**: `PatientName`, `Diagnosis` for data validation
- **Repository Interfaces**: `IPatientRepository`, `IDiagnosticResultRepository`

### Application Layer (Use Cases)
- **Commands**: `CreatePatientCommand`, `UpdateDiagnosisCommand`, etc.
- **Queries**: `GetPatientsQuery`, `GetPatientDetailsQuery`, etc.
- **Handlers**: Command and Query handlers implementing CQRS pattern
- **DTOs**: Data transfer objects for inter-layer communication

### Infrastructure Layer (External Concerns)
- **Data Access**: EF Core with PostgreSQL
- **Repositories**: Repository interface implementations
- **Authentication**: JWT-based authentication

### Presentation Layer (API & Frontend)
- **GraphQL API**: HotChocolate GraphQL server
- **Frontend**: TypeScript React with Next.js

## 🛠️ Technologies

### Backend
- **.NET 10.0** with HotChocolate GraphQL
- **Entity Framework Core** with PostgreSQL
- **JWT Authentication**
- **Docker** for containerization

### Frontend
- **React 18** with TypeScript
- **Next.js 14** with App Router
- **Apollo Client** for GraphQL
- **Tailwind CSS** for styling
- **Responsive design** with mobile-first approach

## 📁 Project Structure

```
HealthHub/
├── Frontend/                 # TypeScript React frontend
│   ├── app/                 # Next.js App Router
│   ├── components/          # React components
│   ├── lib/                 # Utilities and configuration
│   └── package.json         # Frontend dependencies
├── HealthHub/               # ASP.NET Core backend
│   ├── Application/         # CQRS handlers
│   ├── Domain/              # Business entities
│   ├── Infrastructure/      # Data access
│   ├── Presentation/        # GraphQL API
│   ├── wwwroot/             # Built frontend
│   └── Program.cs           # Main entry point
├── compose.yaml             # Docker Compose configuration
└── README.md               # This file
```

## ⚙️ Configuration

### Environment Variables
- `DB_CONNECTION`: PostgreSQL connection string
- `JWT_KEY`: Secret key for JWT tokens
- `ASPNETCORE_ENVIRONMENT`: Environment (Development/Production)

### Default Configuration (`appsettings.json`)
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

## 📚 Documentation

- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Detailed technical architecture
- **[API.md](API.md)** - Complete GraphQL API reference
- **[DEPLOYMENT.md](DEPLOYMENT.md)** - Deployment guides
- **[DEVELOPMENT.md](DEVELOPMENT.md)** - Development workflow

## 🧪 Testing

```bash
# Run tests
cd HealthHub
dotnet test

# TypeScript checking
cd Frontend
npm run type-check

# Linting
npm run lint
```

## 🐳 Docker

### Build image:
```bash
docker build -t healthhub -f HealthHub/Dockerfile .
```

### Run with PostgreSQL:
```bash
docker-compose up -d
```

## 🔒 Security

- JWT-based authentication with 12-hour expiration
- Input validation at all levels
- Secure configuration management
- Health checks for monitoring

## 🤝 Contributing

See [DEVELOPMENT.md](DEVELOPMENT.md) for detailed contribution guidelines.

## 📄 License

This project is created for demonstration purposes.

---

**HealthHub** - Modern patient management solution with GraphQL API and TypeScript React frontend.