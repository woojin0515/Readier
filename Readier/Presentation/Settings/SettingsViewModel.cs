using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Readier.Interfaces;

namespace Readier.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly IUserPreferencesService _preferences;
    private bool _isLoading = true;

    [ObservableProperty]
    private bool notificationsEnabled = true;

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
            NotificationsEnabled = prefs.NotificationsEnabled;
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
        prefs.NotificationsEnabled = NotificationsEnabled;
        await _preferences.SaveAsync(prefs);
    }
}
