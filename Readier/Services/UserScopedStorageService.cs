using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Readier.Infrastructure.Persistence;
using Readier.Interfaces;

namespace Readier.Services;

public class UserScopedStorageService : IStorageService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IDbContextFactory<ReadierDbContext> _dbContextFactory;
    private readonly PreferencesStorageService _localStorage;

    public UserScopedStorageService(
        AuthenticationStateProvider authenticationStateProvider,
        IDbContextFactory<ReadierDbContext> dbContextFactory,
        PreferencesStorageService localStorage)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _dbContextFactory = dbContextFactory;
        _localStorage = localStorage;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var userId = await GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
            return await _localStorage.GetAsync<T>(key);

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var record = await db.UserStorageRecords
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == userId && x.StorageKey == key);

        if (record is null)
        {
            var localValue = await _localStorage.GetAsync<T>(key);
            if (localValue is not null)
            {
                await UpsertAsync(db, userId, key, localValue);
                await _localStorage.RemoveAsync(key);
            }

            return localValue;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(record.JsonValue);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value)
    {
        var userId = await GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await _localStorage.SetAsync(key, value);
            return;
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        await UpsertAsync(db, userId, key, value);
    }

    public async Task RemoveAsync(string key)
    {
        var userId = await GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await _localStorage.RemoveAsync(key);
            return;
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var record = await db.UserStorageRecords
            .SingleOrDefaultAsync(x => x.UserId == userId && x.StorageKey == key);

        if (record is null)
            return;

        db.UserStorageRecords.Remove(record);
        await db.SaveChangesAsync();
    }

    private static async Task UpsertAsync<T>(ReadierDbContext db, string userId, string key, T value)
    {
        var serialized = JsonSerializer.Serialize(value);
        var record = await db.UserStorageRecords
            .SingleOrDefaultAsync(x => x.UserId == userId && x.StorageKey == key);

        if (record is null)
        {
            db.UserStorageRecords.Add(new UserStorageRecord
            {
                UserId = userId,
                StorageKey = key,
                JsonValue = serialized,
                UpdatedUtc = DateTime.UtcNow
            });
        }
        else
        {
            record.JsonValue = serialized;
            record.UpdatedUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    private async Task<string?> GetUserIdAsync()
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = state.User;
        if (user.Identity?.IsAuthenticated != true)
            return null;

        return user.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? user.FindFirstValue("sub");
    }
}
