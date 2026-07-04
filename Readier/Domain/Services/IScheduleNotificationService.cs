using Readier.Models;

namespace Readier.Interfaces;

public interface IScheduleNotificationService
{
    Task ScheduleAsync(Schedule schedule);

    Task CancelAsync(Guid scheduleId);

    Task<bool> ShowPreviewAsync(bool useCalmCopy);
}
