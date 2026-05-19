using Readier.ViewModels;

namespace Readier.Views;

public partial class ScheduleListPage : ContentPage
{
    public ScheduleListViewModel ViewModel { get; }

    public ScheduleListPage(ScheduleListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = ViewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadCommand.ExecuteAsync(null);
    }
}
