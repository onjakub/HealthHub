using HealthHub.Maui.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HealthHub.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Create main window with AppShell
        var window = new Window(new AppShell());
        Application.Current?.OpenWindow(window);

        // Initialize authentication service to load saved token
        var authService = Handler?.MauiContext?.Services?.GetService<IAuthService>();
        authService?.LoadTokenAsync();
    }
}