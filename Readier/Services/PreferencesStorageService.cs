using System.Text.Json;
using Readier.Interfaces;

namespace Readier.Services;

public class PreferencesStorageService : IStorageService
{
    public Task<T?> GetAsync<T>(string key)
    {
        var json = Preferences.Default.Get(key, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
            return Task.FromResult<T?>(default);

        try
        {
            return Task.FromResult(JsonSerializer.Deserialize<T>(json));
        }
        catch (JsonException)
        {
            return Task.FromResult<T?>(default);
        }
    }

    public Task SetAsync<T>(string key, T value)
    {
        Preferences.Default.Set(key, JsonSerializer.Serialize(value));
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        Preferences.Default.Remove(key);
        return Task.CompletedTask;
    }
}
