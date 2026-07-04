using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Readier.Infrastructure.Persistence;
using Readier.Interfaces;
using Readier.Models;

namespace Readier.Services;

public class UserScopedPlanRepository : IPlanRepository
{
    private const string LegacyStorageKey = "readier.schedules.v1";

    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IDbContextFactory<ReadierDbContext> _dbContextFactory;
    private readonly IStorageService _storage;

    public UserScopedPlanRepository(
        AuthenticationStateProvider authenticationStateProvider,
        IDbContextFactory<ReadierDbContext> dbContextFactory,
        IStorageService storage)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _dbContextFactory = dbContextFactory;
        _storage = storage;
    }

    public async Task<IReadOnlyList<Schedule>> ListAsync()
    {
        var userId = await GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
            return await ReadFromLegacyStorageAsync();

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var records = await db.Plans
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.StartTime)
            .ToListAsync();

        if (records.Count > 0)
            return records.Select(ToSchedule).ToList();

        var legacy = await ReadFromLegacyStorageAsync();
        if (legacy.Count == 0)
            return legacy;

        foreach (var schedule in legacy)
        {
            await UpsertAsync(db, userId, schedule);
        }

        await _storage.RemoveAsync(LegacyStorageKey);
        return legacy.OrderBy(x => x.StartTime).ToList();
    }

    public async Task<Schedule?> FindAsync(Guid scheduleId)
    {
        var userId = await GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            var legacy = await ReadFromLegacyStorageAsync();
            return legacy.FirstOrDefault(x => x.Id == scheduleId);
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var record = await db.Plans
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == userId && x.PlanId == scheduleId);
        return record is null ? null : ToSchedule(record);
    }

    public async Task SaveAsync(Schedule schedule)
    {
        var userId = await GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            var legacy = await ReadFromLegacyStorageAsync();
            var index = legacy.FindIndex(x => x.Id == schedule.Id);
            if (index >= 0)
                legacy[index] = schedule;
            else
                legacy.Add(schedule);

            await _storage.SetAsync(LegacyStorageKey, legacy);
            return;
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        await UpsertAsync(db, userId, schedule);
    }

    public async Task DeleteAsync(Guid scheduleId)
    {
        var userId = await GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            var legacy = await ReadFromLegacyStorageAsync();
            legacy.RemoveAll(x => x.Id == scheduleId);
            await _storage.SetAsync(LegacyStorageKey, legacy);
            return;
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var record = await db.Plans.SingleOrDefaultAsync(x => x.UserId == userId && x.PlanId == scheduleId);
        if (record is null)
            return;

        db.Plans.Remove(record);
        await db.SaveChangesAsync();
    }

    private async Task<List<Schedule>> ReadFromLegacyStorageAsync()
        => await _storage.GetAsync<List<Schedule>>(LegacyStorageKey) ?? new List<Schedule>();

    private static async Task UpsertAsync(ReadierDbContext db, string userId, Schedule schedule)
    {
        var record = await db.Plans.SingleOrDefaultAsync(x => x.UserId == userId && x.PlanId == schedule.Id);
        if (record is null)
        {
            db.Plans.Add(new PlanRecord
            {
                UserId = userId,
                PlanId = schedule.Id,
                Title = schedule.Title.Trim(),
                OriginJson = SerializePlace(schedule.Origin),
                DestinationJson = SerializePlace(schedule.Destination),
                StartTime = schedule.StartTime,
                EstimatedTravelMinutes = schedule.EstimatedTravelMinutes,
                EstimatedPrepMinutes = schedule.EstimatedPrepMinutes,
                Transportation = schedule.Transportation,
                UpdatedUtc = DateTime.UtcNow
            });
        }
        else
        {
            record.Title = schedule.Title.Trim();
            record.OriginJson = SerializePlace(schedule.Origin);
            record.DestinationJson = SerializePlace(schedule.Destination);
            record.StartTime = schedule.StartTime;
            record.EstimatedTravelMinutes = schedule.EstimatedTravelMinutes;
            record.EstimatedPrepMinutes = schedule.EstimatedPrepMinutes;
            record.Transportation = schedule.Transportation;
            record.UpdatedUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    private static Schedule ToSchedule(PlanRecord record)
    {
        return new Schedule
        {
            Id = record.PlanId,
            Title = record.Title,
            Origin = DeserializePlace(record.OriginJson),
            Destination = DeserializePlace(record.DestinationJson),
            StartTime = record.StartTime,
            EstimatedTravelMinutes = record.EstimatedTravelMinutes,
            EstimatedPrepMinutes = record.EstimatedPrepMinutes,
            Transportation = record.Transportation
        };
    }

    private static string? SerializePlace(Place? place)
        => place is null ? null : JsonSerializer.Serialize(place);

    private static Place? DeserializePlace(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Place>(json);
        }
        catch (JsonException)
        {
            return null;
        }
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
