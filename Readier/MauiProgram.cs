using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
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
                .UseLocalNotification()
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
            services.AddSingleton(_ => new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            });

            services.AddSingleton<IStorageService, PreferencesStorageService>();
            services.AddSingleton<IUserPreferencesService, UserPreferencesService>();
            services.AddSingleton<ILeaveTimeCalculator, LeaveTimeCalculator>();
            services.AddSingleton<IScheduleNotificationService, LocalNotificationService>();
            services.AddSingleton<IPlaceSearchService, KakaoPlaceSearchService>();
            services.AddSingleton<ITravelTimeProvider, KakaoTravelTimeProvider>();
        }

        private static void RegisterViewModels(IServiceCollection services)
        {
            services.AddTransient<ScheduleListViewModel>();
            services.AddTransient<ScheduleEditViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<BehaviorAnalysisViewModel>();
        }

        private static void RegisterPages(IServiceCollection services)
        {
            services.AddTransient<ScheduleListPage>();
            services.AddTransient<ScheduleEditPage>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<BehaviorAnalysisPage>();
        }
    }
}
