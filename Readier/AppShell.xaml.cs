using Readier.Views;

namespace Readier
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ScheduleEditPage), typeof(ScheduleEditPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        }

        private async void OnSettingsMenuClicked(object? sender, EventArgs e)
        {
            Current.FlyoutIsPresented = false;
            await Current.GoToAsync(nameof(SettingsPage));
        }
    }
}
