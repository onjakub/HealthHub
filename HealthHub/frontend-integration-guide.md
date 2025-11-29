# HealthHub - Integrace TypeScript React frontendu s ASP.NET Core

Tento dokument popisuje kompletní integraci TypeScript React frontendu s existující ASP.NET Core aplikací.

## Architektura integrace

### Frontend (React + TypeScript)
- **Framework**: Next.js 14 s App Router
- **Styling**: Tailwind CSS
- **Stavové řízení**: React Hooks
- **API komunikace**: Apollo Client pro GraphQL
- **Build systém**: Next.js static export

### Backend (ASP.NET Core)
- **API**: GraphQL s HotChocolate
- **Middleware**: Statické soubory + SPA routing
- **Autentizace**: JWT Bearer tokeny
- **Databáze**: PostgreSQL s Entity Framework Core

## Struktura projektu

```
HealthHub/
├── wwwroot/                    # React frontend zdrojové kódy
│   ├── app/                    # Next.js App Router
│   │   ├── layout.tsx          # Hlavní layout aplikace
│   │   ├── page.tsx            # Hlavní stránka (přesměrování)
│   │   └── patients/           # Stránka správy pacientů
│   ├── components/             # React komponenty
│   ├── lib/                    # Utility a konfigurace
│   ├── services/               # Service vrstva pro API
│   └── package.json            # Frontend dependencies
├── Program.cs                  # ASP.NET Core konfigurace
├── build-frontend.ps1          # Windows build script
├── build-frontend.sh           # Linux/macOS build script
└── wwwroot/                    # Sestavené statické soubory (výstup)
```

## Vývojové prostředí

### Požadavky
- Node.js 18+
- .NET 7+
- npm nebo yarn

### Spuštění vývojového serveru

#### 1. Backend (ASP.NET Core)
```bash
cd HealthHub
dotnet run
```
Backend běží na: `http://localhost:5000`

#### 2. Frontend (React dev server)
```bash
cd HealthHub/wwwroot
npm install
npm run dev
```
Frontend běží na: `http://localhost:3000`

### Hot-reload během vývoje
- React dev server podporuje hot-reload
- GraphQL API je dostupné na `/graphql`
- CORS je nakonfigurován pro `localhost:3000`

## Produkční sestavení

### 1. Sestavení React frontendu
```bash
# Windows
.\build-frontend.ps1

# Linux/macOS
chmod +x build-frontend.sh
./build-frontend.sh
```

### 2. Sestavení ASP.NET Core aplikace
```bash
dotnet build
dotnet publish -c Release
```

### 3. Spuštění produkční aplikace
```bash
dotnet run --environment Production
```

## Konfigurace middleware

ASP.NET Core aplikace obsahuje middleware pro:

### Statické soubory
- Soubory z `wwwroot` jsou servírovány jako statický obsah
- Cacheování v produkčním prostředí (1 hodina)
- Komprese a optimalizace

### SPA routing
- Neznámé cesty přesměrovány na `index.html`
- Podpora React Router client-side routing
- Výjimky pro API endpoints (`/api/*`, `/graphql`)

### CORS konfigurace
```csharp
app.UseCors(policy => policy
    .WithOrigins("http://localhost:3000", "https://localhost:3000")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());
```

## API komunikace

### GraphQL endpoint
- **Cesta**: `/graphql`
- **Autentizace**: JWT Bearer tokeny
- **Schéma**: HotChocolate s filtrováním a řazením

### Příklady GraphQL operací

#### Načtení pacientů
```graphql
query GetPatients {
  patients {
    id
    firstName
    lastName
    dateOfBirth
    diagnosticResults {
      id
      diagnosis
      date
    }
  }
}
```

#### Přidání pacienta
```graphql
mutation CreatePatient($input: CreatePatientInput!) {
  createPatient(input: $input) {
    id
    firstName
    lastName
  }
}
```

## Deployment

### Docker
Aplikace je připravena pro Docker deployment s multi-stage build:

```dockerfile
# Build React frontend
FROM node:18-alpine AS frontend-build
WORKDIR /app
COPY wwwroot/ ./
RUN npm install && npm run build:static

# Build ASP.NET Core aplikace
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS backend-build
WORKDIR /src
COPY . ./
RUN dotnet publish -c Release -o /app

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=backend-build /app ./
COPY --from=frontend-build /app/out ./wwwroot/
ENTRYPOINT ["dotnet", "HealthHub.dll"]
```

### Manuální deployment
1. Sestavit frontend: `./build-frontend.sh`
2. Publikovat backend: `dotnet publish -c Release`
3. Nakopírovat výstup na server
4. Spustit: `dotnet HealthHub.dll`

## Monitorování a logování

### Health checks
- Endpoint: `/health`
- Kontrola databázového připojení
- Stav API služeb

### Logování
- ASP.NET Core built-in logging
- Konfigurovatelné úrovně (Information, Warning, Error)

## Bezpečnost

### Autentizace
- JWT tokeny s 12hodinovou expirací
- Token endpoint: `/auth/token`
- Role-based authorization

### HTTPS
- V produkčním prostředí povinné
- Konfigurace v `appsettings.Production.json`

## Troubleshooting

### Časté problémy

#### Frontend se nenačte
- Zkontrolovat build frontendu
- Ověřit, že `index.html` existuje v `wwwroot`
- Kontrola konzole pro chyby

#### API volání selhávají
- Ověřit CORS konfiguraci
- Kontrola GraphQL endpointu na `/graphql`
- Ověřit JWT tokeny

#### Hot-reload nefunguje
- Spustit React dev server na portu 3000
- Ověřit CORS pro `localhost:3000`
- Kontrola network requests

### Logy a debugování
```bash
# ASP.NET Core logy
dotnet run --verbosity detailed

# Frontend logy (dev server)
npm run dev
```

## Další zdroje

- [Next.js dokumentace](https://nextjs.org/docs)
- [ASP.NET Core dokumentace](https://docs.microsoft.com/aspnet/core)
- [HotChocolate GraphQL](https://chillicream.com/docs/hotchocolate)
- [Apollo Client](https://www.apollographql.com/docs/react/)