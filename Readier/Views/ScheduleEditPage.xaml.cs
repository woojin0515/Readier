using Readier.ViewModels;

namespace Readier.Views;

public partial class ScheduleEditPage : ContentPage
{
    public ScheduleEditViewModel ViewModel { get; }

    public ScheduleEditPage(ScheduleEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = ViewModel = viewModel;
    }
}
