using HealthHub.Maui.Services;

namespace HealthHub.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
        
        // Initialize authentication service to load saved token
        var authService = Services.GetService<IAuthService>();
        authService?.LoadTokenAsync();
    }
}