using Readier.Interfaces;
using Readier.Models;

namespace Readier.Services;

public class UserPreferencesService : IUserPreferencesService
{
    private const string StorageKey = "readier.preferences.v1";

    private readonly IStorageService _storage;
    private UserPreferences? _cached;

    public event EventHandler? PreferencesChanged;

    public UserPreferencesService(IStorageService storage)
    {
        _storage = storage;
    }

    public async Task<UserPreferences> GetAsync()
    {
        if (_cached is not null) return _cached;
        _cached = await _storage.GetAsync<UserPreferences>(StorageKey) ?? new UserPreferences();
        return _cached;
    }

    public async Task SaveAsync(UserPreferences preferences)
    {
        _cached = preferences;
        await _storage.SetAsync(StorageKey, preferences);
        PreferencesChanged?.Invoke(this, EventArgs.Empty);
    }
}
