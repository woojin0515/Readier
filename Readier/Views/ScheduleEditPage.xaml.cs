using Readier.ViewModels;

namespace Readier.Views;

public partial class ScheduleEditPage : ContentPage
{
    public ScheduleEditPage(ScheduleEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
