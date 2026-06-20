using System.Text.Json;
using Microsoft.JSInterop;
using Readier.Interfaces;

namespace Readier.Services;

public class PreferencesStorageService : IStorageService
{
    private readonly IJSRuntime _js;

    public PreferencesStorageService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await _js.InvokeAsync<string?>("readierStorage.getItem", key);
        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    public Task SetAsync<T>(string key, T value)
        => _js.InvokeVoidAsync("readierStorage.setItem", key, JsonSerializer.Serialize(value)).AsTask();

    public Task RemoveAsync(string key)
        => _js.InvokeVoidAsync("readierStorage.removeItem", key).AsTask();
}
