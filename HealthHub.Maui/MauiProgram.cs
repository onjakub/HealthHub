using Microsoft.Extensions.Logging;
using HealthHub.Maui.ViewModels;
using HealthHub.Maui.Views;
using HealthHub.Maui.Services;
using HealthHub.Maui.Models;
using GraphQL.Client;
using GraphQL.Client.Http;
using Newtonsoft.Json;
using System;
using CommunityToolkit.Maui;

namespace HealthHub.Maui;

public static class MauiProgram
{
    [STAThread]
    public static void Main(string[] args)
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        var app = builder.Build();
    }

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Register services
        RegisterServices(builder.Services);

        // Register ViewModels
        RegisterViewModels(builder.Services);

        // Register Views
        RegisterViews(builder.Services);

        return builder.Build();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        // GraphQL Client - simplified for .NET 10 compatibility
        services.AddSingleton<HttpClient>(provider =>
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5000/graphql/");
            return httpClient;
        });

        // Application services
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IPatientService>(provider =>
            new PatientService(provider.GetRequiredService<HttpClient>(), provider.GetRequiredService<IAuthService>()));
        services.AddSingleton<IDiagnosticResultService>(provider =>
            new DiagnosticResultService(provider.GetRequiredService<HttpClient>(), provider.GetRequiredService<IAuthService>()));
        services.AddSingleton<ISettingsService, SettingsService>();
    }

    private static void RegisterViewModels(IServiceCollection services)
    {
        services.AddTransient<PatientsViewModel>();
        services.AddTransient<PatientDetailViewModel>();
        services.AddTransient<AddPatientViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
    }

    private static void RegisterViews(IServiceCollection services)
    {
        services.AddTransient<PatientsPage>();
        services.AddTransient<PatientDetailPage>();
        services.AddTransient<AddPatientPage>();
        services.AddTransient<LoginPage>();
        services.AddTransient<MainPage>();
    }
}