using Readier.Models;

namespace Readier.Services;

public interface IScheduleStore
{
    Task<IReadOnlyList<Schedule>> GetAllAsync();

    Task<Schedule?> GetAsync(Guid id);

    Task SaveAsync(Schedule schedule);

    Task DeleteAsync(Guid id);
}
