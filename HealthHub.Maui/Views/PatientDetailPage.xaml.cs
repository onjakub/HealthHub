using HealthHub.Maui.ViewModels;

namespace HealthHub.Maui.Views;

public partial class PatientDetailPage : ContentPage
{
    public PatientDetailPage(PatientDetailViewModel viewModel)
    {
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is PatientDetailViewModel viewModel)
        {
            await viewModel.LoadPatientDetailsCommand.ExecuteAsync(null);
        }
    }
}