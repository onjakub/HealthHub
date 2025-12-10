using HealthHub.Maui.ViewModels;

namespace HealthHub.Maui.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        BindingContext = viewModel;
    }
}