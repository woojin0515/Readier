using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Readier.Infrastructure.Persistence;
using Readier.Interfaces;
using Readier.Services;
using Readier.ViewModels;

var builder = WebApplication.CreateBuilder(args);
var dbConnectionString = ResolveDbConnectionString(builder.Configuration);
var googleClientId = builder.Configuration["Authentication:Google:ClientId"]
                     ?? Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
                         ?? Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthorization();
var authBuilder = builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
    });

if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
    });
}

builder.Services.AddHttpClient<IPlaceSearchService, KakaoPlaceSearchService>();
builder.Services.AddHttpClient<ITravelTimeProvider, KakaoTravelTimeProvider>();
builder.Services.AddScoped<PreferencesStorageService>();
if (!string.IsNullOrWhiteSpace(dbConnectionString))
{
    builder.Services.AddDbContextFactory<ReadierDbContext>(options =>
        options.UseSqlServer(
            dbConnectionString,
            sql =>
            {
                sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                sql.CommandTimeout(30);
            }));
    builder.Services.AddScoped<IStorageService, UserScopedStorageService>();
    builder.Services.AddScoped<IPlanRepository, UserScopedPlanRepository>();
}
else
{
    builder.Services.AddScoped<IStorageService, PreferencesStorageService>();
    builder.Services.AddScoped<IPlanRepository, StoragePlanRepository>();
}

builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
builder.Services.AddScoped<ILeaveTimeCalculator, LeaveTimeCalculator>();
builder.Services.AddScoped<IScheduleNotificationService, LocalNotificationService>();
builder.Services.AddTransient<ScheduleListViewModel>();
builder.Services.AddTransient<ScheduleEditViewModel>();
builder.Services.AddTransient<SettingsViewModel>();
builder.Services.AddTransient<BehaviorAnalysisViewModel>();

var app = builder.Build();

if (!string.IsNullOrWhiteSpace(dbConnectionString))
{
    await using var scope = app.Services.CreateAsyncScope();
    await using var db = await scope.ServiceProvider.GetRequiredService<IDbContextFactory<ReadierDbContext>>()
        .CreateDbContextAsync();
    await db.Database.EnsureCreatedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/auth/login", async (HttpContext httpContext, string? returnUrl) =>
{
    if (string.IsNullOrWhiteSpace(googleClientId) || string.IsNullOrWhiteSpace(googleClientSecret))
    {
        httpContext.Response.Redirect("/");
        return;
    }

    var target = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
    await httpContext.ChallengeAsync(
        GoogleDefaults.AuthenticationScheme,
        new() { RedirectUri = target });
});

app.MapGet("/auth/logout", async (HttpContext httpContext, string? returnUrl) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    httpContext.Response.Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
});

app.MapGet("/auth/switch", async (HttpContext httpContext, string? returnUrl) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    if (string.IsNullOrWhiteSpace(googleClientId) || string.IsNullOrWhiteSpace(googleClientSecret))
    {
        httpContext.Response.Redirect("/");
        return;
    }

    var target = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
    var props = new AuthenticationProperties { RedirectUri = target };
    props.Parameters["prompt"] = "select_account";
    await httpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, props);
});

app.MapRazorComponents<global::Readier.App>()
    .AddInteractiveServerRenderMode();

app.Run();

static string? ResolveDbConnectionString(IConfiguration configuration)
{
    var candidates = new[]
    {
        configuration.GetConnectionString("ReadierDb"),
        configuration.GetConnectionString("readierdb"),
        Environment.GetEnvironmentVariable("READIER_DB_CONNECTION"),
        Environment.GetEnvironmentVariable("ConnectionStrings__ReadierDb"),
        Environment.GetEnvironmentVariable("ConnectionStrings__readierdb"),
        Environment.GetEnvironmentVariable("SQLAZURECONNSTR_readierdb"),
        Environment.GetEnvironmentVariable("SQLCONNSTR_readierdb")
    };

    return candidates.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
}
