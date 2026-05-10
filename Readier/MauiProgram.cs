using Microsoft.Extensions.Logging;
using Readier.Interfaces;
using Readier.Services;
using Readier.ViewModels;
using Readier.Views;

namespace Readier
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            RegisterServices(builder.Services);
            RegisterViewModels(builder.Services);
            RegisterPages(builder.Services);

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IStorageService, PreferencesStorageService>();
        }

        private static void RegisterViewModels(IServiceCollection services)
        {
            services.AddTransient<ScheduleListViewModel>();
            services.AddTransient<ScheduleEditViewModel>();
        }

        private static void RegisterPages(IServiceCollection services)
        {
            services.AddTransient<ScheduleListPage>();
            services.AddTransient<ScheduleEditPage>();
        }
    }
}
