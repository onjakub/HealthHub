using HealthHub.Maui.ViewModels;

namespace HealthHub.Maui.Views;

public partial class AddPatientPage : ContentPage
{
    public AddPatientPage(AddPatientViewModel viewModel)
    {
        BindingContext = viewModel;
    }
}