using Readier.Interfaces;
using Readier.Models;

namespace Readier.Services;

public class StoragePlanRepository : IPlanRepository
{
    private const string StorageKey = "readier.schedules.v1";

    private readonly IStorageService _storage;

    public StoragePlanRepository(IStorageService storage)
    {
        _storage = storage;
    }

    public async Task<IReadOnlyList<Schedule>> ListAsync()
        => await _storage.GetAsync<List<Schedule>>(StorageKey) ?? new List<Schedule>();

    public async Task<Schedule?> FindAsync(Guid scheduleId)
    {
        var list = await _storage.GetAsync<List<Schedule>>(StorageKey) ?? new List<Schedule>();
        return list.FirstOrDefault(x => x.Id == scheduleId);
    }

    public async Task SaveAsync(Schedule schedule)
    {
        var list = await _storage.GetAsync<List<Schedule>>(StorageKey) ?? new List<Schedule>();
        var index = list.FindIndex(x => x.Id == schedule.Id);
        if (index >= 0)
            list[index] = schedule;
        else
            list.Add(schedule);

        await _storage.SetAsync(StorageKey, list);
    }

    public async Task DeleteAsync(Guid scheduleId)
    {
        var list = await _storage.GetAsync<List<Schedule>>(StorageKey) ?? new List<Schedule>();
        list.RemoveAll(x => x.Id == scheduleId);
        await _storage.SetAsync(StorageKey, list);
    }
}
