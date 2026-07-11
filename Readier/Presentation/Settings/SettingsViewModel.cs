using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Readier.Interfaces;
using Readier.Models;

namespace Readier.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly IUserPreferencesService _preferences;
    private bool _isLoading = true;

    public IReadOnlyList<int> ReminderMinuteOptions { get; } = NotificationInteractionSpec.LeaveSoonOptions;
    public IReadOnlyList<int> SnoozeMinuteOptions { get; } = NotificationInteractionSpec.SnoozePresetOptions;

    [ObservableProperty]
    private bool notificationsEnabled = true;

    [ObservableProperty]
    private string displayName = string.Empty;

    [ObservableProperty]
    private int leaveSoonReminderMinutes = 10;

    [ObservableProperty]
    private int snoozePresetMinutes = 10;

    [ObservableProperty]
    private bool useCalmReminderCopy = true;

    public SettingsViewModel(IUserPreferencesService preferences)
    {
        _preferences = preferences;
        Title = "설정";
        PropertyChanged += OnAnyPropertyChanged;
    }

    public async Task LoadAsync()
    {
        _isLoading = true;
        try
        {
            var prefs = await _preferences.GetAsync();
            DisplayName = prefs.DisplayName;
            NotificationsEnabled = prefs.NotificationsEnabled;
            LeaveSoonReminderMinutes = prefs.Notification.LeaveSoonReminderMinutes;
            SnoozePresetMinutes = prefs.Notification.SnoozePresetMinutes;
            UseCalmReminderCopy = prefs.Notification.UseCalmReminderCopy;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async void OnAnyPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isLoading) return;
        if (e.PropertyName is nameof(Title) or nameof(IsBusy)) return;
        await SaveCurrentAsync();
    }

    private async Task SaveCurrentAsync()
    {
        var prefs = await _preferences.GetAsync();
        prefs.DisplayName = DisplayName.Trim();
        prefs.NotificationsEnabled = NotificationsEnabled;
        prefs.Notification.LeaveSoonReminderMinutes = LeaveSoonReminderMinutes;
        prefs.Notification.SnoozePresetMinutes = SnoozePresetMinutes;
        prefs.Notification.UseCalmReminderCopy = UseCalmReminderCopy;
        await _preferences.SaveAsync(prefs);
    }
}
