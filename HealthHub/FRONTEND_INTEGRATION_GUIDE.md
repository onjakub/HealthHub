# HealthHub - React Frontend Integration Guide

This document describes the complete integration of TypeScript React frontend with ASP.NET Core backend.

## Architecture Overview

The integration follows a hybrid approach:
- **Production**: React frontend is built as static files and served by ASP.NET Core static file middleware
- **Development**: React runs on Next.js dev server with hot-reload, proxying API calls to ASP.NET Core

## Project Structure

```
HealthHub/
├── wwwroot/                 # React frontend source
│   ├── app/                # Next.js app directory
│   ├── components/         # React components
│   ├── lib/               # API clients and utilities
│   ├── public/            # Static assets
│   └── package.json       # Frontend dependencies
├── Presentation/
│   └── Controllers/       # REST API controllers
├── Program.cs             # ASP.NET Core configuration
└── build-frontend.*       # Build scripts
```

## Development Setup

### Option 1: Integrated Development (Recommended)

1. **Start ASP.NET Core backend:**
   ```bash
   cd HealthHub
   dotnet run
   ```
   Backend runs on http://localhost:5023

2. **Start React frontend with proxy:**
   ```bash
   cd HealthHub/wwwroot
   npm install
   npm run dev
   ```
   Frontend runs on http://localhost:3000 with API proxying to backend

### Option 2: Separate Development Servers

1. **Start ASP.NET Core backend:**
   ```bash
   cd HealthHub
   dotnet run
   ```

2. **Start React frontend:**
   ```bash
   cd HealthHub/wwwroot
   npm install
   npm run dev
   ```

3. **Start proxy server (optional):**
   ```bash
   cd HealthHub/wwwroot
   npm install -g concurrently
   npm run dev:proxy
   ```

## Production Build

### Build Process

1. **Build React frontend and copy to ASP.NET Core:**
   ```bash
   # Using PowerShell (Windows)
   .\build-frontend.ps1

   # Using Bash (Linux/macOS)
   ./build-frontend.sh
   ```

2. **Run the application:**
   ```bash
   dotnet run
   ```

### Manual Build Steps

1. Build React frontend:
   ```bash
   cd wwwroot
   npm run build:static
   ```

2. Build ASP.NET Core application:
   ```bash
   dotnet build --configuration Release
   ```

## API Integration

### REST API Endpoints

The React frontend communicates with the backend through REST API:

- `GET /api/patients` - Get all patients
- `GET /api/patients/{id}` - Get patient by ID
- `GET /api/patients/count` - Get patient count
- `POST /auth/token` - Authentication

### API Client

The React frontend uses `lib/api-client.ts` which automatically handles:
- Environment detection (development vs production)
- Authentication token management
- Error handling

## Configuration Details

### ASP.NET Core Configuration (Program.cs)

- Static file serving with `UseStaticFiles()`
- SPA fallback routing with `MapFallbackToFile("/index.html")`
- CORS configuration for development
- REST API controller mapping

### Next.js Configuration (next.config.mjs)

- Static export mode (`output: 'export'`)
- Production path configuration
- Image optimization settings

## Authentication Flow

1. User enters credentials in React frontend
2. Frontend calls `/auth/token` endpoint
3. Backend returns JWT token
4. Frontend stores token and includes it in API requests
5. Backend validates token for protected endpoints

## Development Tips

### Hot Reload

During development, React changes are reflected immediately through Next.js hot reload.

### API Testing

Use the built-in ASP.NET Core OpenAPI endpoint at `/swagger` (if enabled) or test directly:
```bash
curl -X GET http://localhost:5023/api/patients
```

### Debugging

- Frontend logs: Browser developer tools
- Backend logs: ASP.NET Core console output
- Network requests: Browser network tab

## Troubleshooting

### Common Issues

1. **CORS errors**: Ensure backend is running on port 5023
2. **Build failures**: Check Node.js version and dependencies
3. **Static file issues**: Verify build output is copied to correct location
4. **Authentication failures**: Check JWT configuration

### Port Configuration

- Backend: 5023 (configurable in `launchSettings.json`)
- Frontend: 3000 (Next.js default)
- Proxy: 3001 (development only)

## Deployment

The application can be deployed as a single unit:
1. Build frontend and backend using build scripts
2. Deploy the entire HealthHub directory
3. Ensure database connection is configured
4. Run `dotnet HealthHub.dll`

For container deployment, use the provided Dockerfile which includes both frontend and backend.

## Poznámka: adresář `chunks`

- Co to je: `chunks/` obsahuje dočasné JavaScriptové „chunky“ generované Next.js (Turbopack/Webpack) v průběhu vývoje nebo při bězích nástrojů. Typicky soubory jako `turbopack-*.js`.
- K čemu slouží: jde o runtime artefakty pro dev/server bundler (HMR, tracing apod.). Backend je nepotřebuje pro běh produkce.
- Je bezpečné ho smazat?: Ano. Není součástí zdrojového kódu ani produkčního buildu. Pokud se znovu objeví, vytvořil ho dev server/bundler a lze ho ignorovat.
- Verzování: adresář `HealthHub/chunks/` je přidán do `.gitignore`, aby se omylem necommitoval.