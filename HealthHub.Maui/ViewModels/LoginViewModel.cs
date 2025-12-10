using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HealthHub.Maui.Services;

namespace HealthHub.Maui.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "HealthHub Login";
    }

    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            SetError("Please enter both username and password");
            return;
        }

        await ExecuteWithLoading(async () =>
        {
            var success = await _authService.LoginAsync(Username, Password);
            if (success)
            {
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                SetError("Invalid credentials");
            }
        });
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}