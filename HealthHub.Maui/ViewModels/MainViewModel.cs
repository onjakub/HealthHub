using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HealthHub.Maui.Services;

namespace HealthHub.Maui.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private bool _isAuthenticated;

    public MainViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "HealthHub";
        
        // Subscribe to authentication state changes
        _authService.AuthenticationStateChanged += OnAuthenticationStateChanged;
        
        // Check current authentication state
        IsAuthenticated = _authService.IsAuthenticated;
    }

    private void OnAuthenticationStateChanged(object? sender, bool isAuthenticated)
    {
        IsAuthenticated = isAuthenticated;
    }

    [RelayCommand]
    private async Task NavigateToPatients()
    {
        if (IsAuthenticated)
        {
            await Application.Current?.MainPage?.Navigation.PushAsync(
                Application.Current?.Handler?.MauiContext?.Services?.GetService<PatientsPage>());
        }
        else
        {
            await Application.Current?.MainPage?.Navigation.PushAsync(
                Application.Current?.Handler?.MauiContext?.Services?.GetService<LoginPage>());
        }
    }

    [RelayCommand]
    private async Task NavigateToLogin()
    {
        if (!IsAuthenticated)
        {
            await Application.Current?.MainPage?.Navigation.PushAsync(
                Application.Current?.Handler?.MauiContext?.Services?.GetService<LoginPage>());
        }
    }

    [RelayCommand]
    private async Task Logout()
    {
        await _authService.LogoutAsync();
        await Application.Current?.MainPage?.Navigation.PopToRootAsync();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        // Implement refresh logic as needed
    }
}