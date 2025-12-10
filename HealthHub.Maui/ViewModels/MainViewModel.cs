using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HealthHub.Maui.Services;
using HealthHub.Maui.Views;

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
            await Shell.Current.GoToAsync(nameof(PatientsPage));
        }
        else
        {
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }
    }

    [RelayCommand]
    private async Task NavigateToLogin()
    {
        if (!IsAuthenticated)
        {
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }
    }

    [RelayCommand]
    private async Task Logout()
    {
        await _authService.LogoutAsync();
        await Shell.Current.GoToAsync("//MainPage");
    }

    [RelayCommand]
    private async Task Refresh()
    {
        // Implement refresh logic as needed
    }

}