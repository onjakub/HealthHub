# HealthHub - Patient Management System

Kompletní integrace TypeScript React frontendu do ASP.NET Core aplikace s GraphQL API.

## Architektura

- **Frontend**: TypeScript React s Next.js (statický export)
- **Backend**: ASP.NET Core s HotChocolate GraphQL
- **Database**: PostgreSQL s Entity Framework Core
- **Authentication**: JWT tokens
- **Build**: Next.js → statické soubory → ASP.NET Core wwwroot

## Struktura projektu

```
HealthHub/
├── wwwroot/                 # React frontend (statické soubory)
│   ├── app/                # Next.js app directory
│   ├── components/         # React komponenty
│   ├── lib/               # GraphQL klient
│   └── README.md          # Frontend dokumentace
├── Application/           # CQRS handlers
├── Domain/               # Business entities
├── Infrastructure/       # Data access
├── Presentation/         # Controllers & GraphQL
└── Program.cs           # Main entry point
```

## Rychlý start

### 1. Spuštění backendu

```bash
# V kořenovém adresáři projektu
cd HealthHub
dotnet run
```

Backend běží na: **http://localhost:5023**

### 2. Spuštění frontendu (vývojový režim)

```bash
# V druhém terminálu
cd Frontend
npm install
npm run dev
```

Frontend dev server běží na: **http://localhost:3000**

### 3. Produkční build

```bash
# Sestavit React frontend (Next.js statický export a kopie do HealthHub/wwwroot)
cd Frontend
npm run build

# Spustit backend s integrovaným frontendem
cd ..
dotnet run
```

## Kompletní příkazy

### Backend příkazy

```bash
# Spuštění aplikace
dotnet run

# Build aplikace
dotnet build

# Testy
dotnet test

# Database migrace
dotnet ef database update
```

### Frontend příkazy (v adresáři Frontend)

```bash
# Instalace závislostí
npm install

# Vývojový server
npm run dev

# Produkční build (obsah se automaticky zkopíruje do HealthHub/wwwroot)
npm run build

# Spuštění produkčního serveru (Next.js)
npm run start

# Čistící příkazy
npm run clean          # Smazání build adresářů
npm run clean:all      # Kompletní vyčištění
npm run install:clean  # Čistá instalace

# TypeScript kontrola
npm run type-check

# Linting
npm run lint
```

## GraphQL API

Aplikace poskytuje GraphQL API na `/graphql` s JWT autentizací.

### Autentizace

```bash
# Získání JWT tokenu
curl -X POST -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"password"}' \
  http://localhost:5023/auth/token
```

### GraphQL příklady

```graphql
# Query pacientů
{
  patients {
    nodes {
      id
      firstName
      lastName
    }
  }
}

# Mutace pro vytvoření pacienta
mutation {
  createPatient(command: {
    firstName: "John"
    lastName: "Doe"
    dateOfBirth: "1980-01-01"
  }) {
    id
    firstName
    lastName
  }
}
```

## Konfigurace

### Databáze

Defaultní connection string v `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=healthhub;Username=health;Password=healthpwd"
  }
}
```

### JWT Autentizace

Konfigurace v `appsettings.json`:
```json
{
  "Jwt": {
    "Key": "dev-secret-change-me-please-very-long",
    "Issuer": "HealthHub",
    "Audience": "HealthHubAudience"
  }
}
```

## Troubleshooting

### Port konflikty

```bash
# Zastavit proces na portu 5023
lsof -ti:5023 | xargs kill -9
```

### NPM problémy

```bash
# Oprava npm cache
sudo chown -R $(whoami) ~/.npm

# Čistá reinstalace
cd HealthHub/wwwroot
npm run install:clean
```

### Database problémy

```bash
# Recreate database
cd HealthHub
dotnet ef database drop --force
dotnet ef database update
```

## Vývojové workflow

1. **Backend vývoj**: Upravovat C# soubory v `HealthHub/` adresáři
2. **Frontend vývoj**: Upravovat TypeScript soubory v `Frontend/`
3. **Hot reload**: Next.js dev server poskytuje instantní aktualizace
4. **Produkční build**: `npm run build` v adresáři `Frontend` (automaticky exportuje a zkopíruje do `HealthHub/wwwroot`)

## Deployment

### Lokální deployment

1. Sestavit frontend: `npm run build` v `Frontend`
2. Spustit backend: `dotnet run` v kořenovém adresáři

### Docker deployment

```bash
# Build Docker image
docker build -t healthhub .

# Spuštění kontejneru
docker run -p 5023:80 healthhub
```

## Funkcionalita

✅ **Kompletní integrace** React + ASP.NET Core  
✅ **GraphQL API** s JWT autentizací  
✅ **SPA routing** pro React Router  
✅ **Hot reload** během vývoje  
✅ **TypeScript** kompilace bez chyb  
✅ **Produkční build** optimalizovaný  

Aplikace je připravena pro další rozšíření funkcionality.