using Readier.ViewModels;

namespace Readier.Views;

public partial class BehaviorAnalysisPage : ContentPage
{
    public BehaviorAnalysisViewModel ViewModel { get; }

    public BehaviorAnalysisPage(BehaviorAnalysisViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = ViewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadAsync();
    }
}
