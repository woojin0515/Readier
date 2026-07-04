using Readier.Models;

namespace Readier.Interfaces;

public interface IPlanRepository
{
    Task<IReadOnlyList<Schedule>> ListAsync();

    Task<Schedule?> FindAsync(Guid scheduleId);

    Task SaveAsync(Schedule schedule);

    Task DeleteAsync(Guid scheduleId);
}
