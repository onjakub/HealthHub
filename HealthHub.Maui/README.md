# HealthHub .NET MAUI Desktop Application

A cross-platform desktop application for healthcare management built with .NET MAUI, designed to work on macOS, Windows, and other supported platforms.

## Overview

This MAUI application provides a native desktop interface for the HealthHub healthcare management system. It connects to the existing HealthHub GraphQL API backend and offers the same functionality as the web frontend but with a native desktop experience.

## Features

- **Cross-platform compatibility** - Runs on macOS, Windows, iOS, and Android
- **Native desktop experience** - Uses MAUI controls for optimal performance
- **GraphQL integration** - Connects to the existing HealthHub GraphQL API
- **Patient management** - Create, view, update, and delete patients
- **Diagnostic management** - Add and manage diagnostic results
- **Authentication** - Secure login with token-based authentication
- **MVVM architecture** - Clean separation of concerns with CommunityToolkit.Mvvm

## Prerequisites

1. **.NET 10 SDK** - Download from [Microsoft .NET](https://dotnet.microsoft.com/download)
2. **Visual Studio** or **Visual Studio Code** with MAUI workload
3. **MAUI Workload** - Install via:
   ```bash
   dotnet workload install maui
   ```
4. **HealthHub Backend** - Ensure the HealthHub GraphQL API is running

## Setup Instructions

### 1. Install MAUI Workload

```bash
# Install MAUI workload
dotnet workload install maui

# Verify installation
dotnet workload list
```

### 2. Restore Dependencies

```bash
cd HealthHub.Maui
dotnet restore
```

### 3. Configure Backend Connection

The application is configured to connect to `http://localhost:5000/graphql/` by default. Ensure your HealthHub backend is running on this endpoint, or modify the `SettingsService` to point to your backend URL.

### 4. Build and Run

#### For macOS Desktop:
```bash
dotnet build -f net10.0-maccatalyst
dotnet run -f net10.0-maccatalyst
```

#### For Windows Desktop:
```bash
dotnet build -f net10.0-windows
dotnet run -f net10.0-windows
```

#### For all platforms:
```bash
dotnet build
dotnet run
```

## Project Structure

```
HealthHub.Maui/
├── App.xaml.cs                 # Application entry point
├── MauiProgram.cs             # Service registration and app configuration
├── AppShell.xaml              # Navigation shell
├── Models/                    # Data models (DTOs)
│   ├── PatientDto.cs
│   ├── PatientDetailDto.cs
│   └── PaginationResponseDto.cs
├── Services/                  # Application services
│   ├── IAuthService.cs
│   ├── IPatientService.cs
│   ├── PatientService.cs
│   ├── DiagnosticResultService.cs
│   └── SettingsService.cs
├── ViewModels/                # MVVM ViewModels
│   ├── BaseViewModel.cs
│   ├── MainViewModel.cs
│   ├── LoginViewModel.cs
│   ├── PatientsViewModel.cs
│   └── PatientDetailViewModel.cs
└── Views/                     # MAUI Pages
    ├── MainPage.xaml
    ├── LoginPage.xaml
    └── PatientsPage.xaml
```

## Key Features

### Authentication
- Demo login system (accepts any username/password)
- Secure token storage using SecureStorage
- Automatic authentication state management

### Patient Management
- View all patients with pagination
- Search patients by name
- Add new patients
- View patient details with diagnostic history

### Diagnostic Management
- Add new diagnostic results
- View diagnostic history per patient
- Notes and timestamp tracking

### Cross-Platform Design
- Responsive UI that works on desktop and mobile
- Platform-specific optimizations
- Consistent user experience across platforms

## API Integration

The MAUI app connects to the existing HealthHub GraphQL API with these endpoints:

- **GraphQL Endpoint**: `http://localhost:5000/graphql/`
- **Authentication**: Bearer token (demo implementation)
- **Queries**: GetPatients, GetPatient, GetPatientDiagnosticResults
- **Mutations**: CreatePatient, UpdatePatient, AddDiagnosticResult

## Configuration

### Backend Connection
Edit `SettingsService.cs` to change the GraphQL endpoint:

```csharp
GraphQLEndpoint = "your-backend-url/graphql/";
```

### Authentication
The current implementation uses a demo authentication system. In a production environment, you would:
1. Implement proper JWT token generation
2. Connect to a real authentication service
3. Handle token refresh and expiration

## Troubleshooting

### Common Issues

1. **MAUI workload not installed**
   ```bash
   dotnet workload install maui
   ```

2. **Backend connection failed**
   - Ensure HealthHub backend is running on localhost:5000
   - Check firewall settings
   - Verify GraphQL endpoint is accessible

3. **Build errors**
   - Run `dotnet restore` to ensure all packages are installed
   - Check .NET 10 SDK is properly installed
   - Verify MAUI workload is installed

### Debugging

Enable debug logging:
```bash
dotnet run --verbosity detailed
```

## Development Notes

### Architecture
- **MVVM Pattern** using CommunityToolkit.Mvvm
- **Dependency Injection** via Microsoft.Extensions.DependencyInjection
- **GraphQL Client** using GraphQL.Client library
- **Navigation** using MAUI Shell

### Performance Considerations
- Async/await patterns for all API calls
- ObservableCollections for UI updates
- Pagination to handle large datasets
- Error handling and user feedback

## Future Enhancements

1. **Offline support** with local database sync
2. **Push notifications** for important updates
3. **Advanced search** and filtering
4. **Reporting** and analytics features
5. **Integration** with healthcare systems (HL7, FHIR)
6. **Multi-language support**

## Support

For issues and questions:
1. Check the troubleshooting section above
2. Review the HealthHub backend documentation
3. Consult .NET MAUI documentation for platform-specific issues

## License

This project follows the same license as the main HealthHub application.