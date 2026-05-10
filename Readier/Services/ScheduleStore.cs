using System.Text.Json;
using Readier.Models;

namespace Readier.Services;

public class ScheduleStore : IScheduleStore
{
    private const string StorageKey = "readier.schedules.v1";

    public Task<IReadOnlyList<Schedule>> GetAllAsync()
        => Task.FromResult<IReadOnlyList<Schedule>>(Load());

    public Task<Schedule?> GetAsync(Guid id)
        => Task.FromResult(Load().FirstOrDefault(s => s.Id == id));

    public Task SaveAsync(Schedule schedule)
    {
        var list = Load();
        var idx = list.FindIndex(s => s.Id == schedule.Id);
        if (idx >= 0)
            list[idx] = schedule;
        else
            list.Add(schedule);

        Persist(list);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        var list = Load();
        list.RemoveAll(s => s.Id == id);
        Persist(list);
        return Task.CompletedTask;
    }

    private static List<Schedule> Load()
    {
        var json = Preferences.Default.Get(StorageKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
            return new List<Schedule>();

        try
        {
            return JsonSerializer.Deserialize<List<Schedule>>(json) ?? new List<Schedule>();
        }
        catch (JsonException)
        {
            return new List<Schedule>();
        }
    }

    private static void Persist(List<Schedule> list)
        => Preferences.Default.Set(StorageKey, JsonSerializer.Serialize(list));
}
