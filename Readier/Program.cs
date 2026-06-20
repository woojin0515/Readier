using Readier.Interfaces;
using Readier.Services;
using Readier.ViewModels;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<IPlaceSearchService, KakaoPlaceSearchService>();
builder.Services.AddHttpClient<ITravelTimeProvider, KakaoTravelTimeProvider>();
builder.Services.AddScoped<IStorageService, PreferencesStorageService>();
builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
builder.Services.AddScoped<ILeaveTimeCalculator, LeaveTimeCalculator>();
builder.Services.AddScoped<IScheduleNotificationService, LocalNotificationService>();
builder.Services.AddTransient<ScheduleListViewModel>();
builder.Services.AddTransient<ScheduleEditViewModel>();
builder.Services.AddTransient<SettingsViewModel>();
builder.Services.AddTransient<BehaviorAnalysisViewModel>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<global::Readier.App>()
    .AddInteractiveServerRenderMode();

app.Run();
