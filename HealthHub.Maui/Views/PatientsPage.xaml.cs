using HealthHub.Maui.ViewModels;

namespace HealthHub.Maui.Views;

public partial class PatientsPage : ContentPage
{
    public PatientsPage(PatientsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is PatientsViewModel viewModel)
        {
            await viewModel.LoadPatientsCommand.ExecuteAsync(null);
        }
    }
}