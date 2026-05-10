using Plugin.LocalNotification;
using Readier.Interfaces;
using Readier.Models;

namespace Readier.Services;

public class LocalNotificationService : IScheduleNotificationService
{
    private readonly ILeaveTimeCalculator _calculator;
    private readonly IUserPreferencesService _preferences;

    public LocalNotificationService(ILeaveTimeCalculator calculator, IUserPreferencesService preferences)
    {
        _calculator = calculator;
        _preferences = preferences;
    }

    public async Task ScheduleAsync(Schedule schedule)
    {
        await CancelAsync(schedule.Id);

        var prefs = await _preferences.GetAsync();
        if (!prefs.NotificationsEnabled)
            return;

        if (!await EnsurePermissionAsync())
            return;

        var plan = _calculator.Calculate(schedule);
        var now = DateTime.Now;
        var destinationLabel = schedule.Destination?.DisplayLine ?? string.Empty;
        var description = string.IsNullOrWhiteSpace(destinationLabel)
            ? schedule.Title
            : $"{schedule.Title} · {destinationLabel}";

        if (plan.StartPrepAt > now)
        {
            await LocalNotificationCenter.Current.Show(new NotificationRequest
            {
                NotificationId = PrepId(schedule.Id),
                Title = "준비 시작 시간이에요",
                Description = description,
                Schedule = new NotificationRequestSchedule { NotifyTime = plan.StartPrepAt }
            });
        }

        if (plan.LeaveAt > now)
        {
            await LocalNotificationCenter.Current.Show(new NotificationRequest
            {
                NotificationId = LeaveId(schedule.Id),
                Title = "출발할 시간이에요",
                Description = description,
                Schedule = new NotificationRequestSchedule { NotifyTime = plan.LeaveAt }
            });
        }
    }

    public Task CancelAsync(Guid scheduleId)
    {
        LocalNotificationCenter.Current.Cancel(PrepId(scheduleId));
        LocalNotificationCenter.Current.Cancel(LeaveId(scheduleId));
        return Task.CompletedTask;
    }

    private static async Task<bool> EnsurePermissionAsync()
    {
        if (await LocalNotificationCenter.Current.AreNotificationsEnabled())
            return true;
        return await LocalNotificationCenter.Current.RequestNotificationPermission();
    }

    private static int PrepId(Guid id) => HashCode.Combine(id, "prep") & 0x7FFFFFFF;

    private static int LeaveId(Guid id) => HashCode.Combine(id, "leave") & 0x7FFFFFFF;
}
