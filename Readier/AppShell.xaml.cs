using Readier.Views;

namespace Readier
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ScheduleEditPage), typeof(ScheduleEditPage));
        }
    }
}
