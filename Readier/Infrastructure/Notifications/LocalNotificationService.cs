using Microsoft.JSInterop;
using Readier.Helpers;
using Readier.Interfaces;
using Readier.Models;

namespace Readier.Services;

public class LocalNotificationService : IScheduleNotificationService
{
    private readonly IJSRuntime _js;
    private readonly ILeaveTimeCalculator _calculator;
    private readonly IUserPreferencesService _preferences;

    public LocalNotificationService(IJSRuntime js, ILeaveTimeCalculator calculator, IUserPreferencesService preferences)
    {
        _js = js;
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
        var now = AppClock.Now;
        var destinationLabel = schedule.Destination?.DisplayLine ?? string.Empty;
        var description = string.IsNullOrWhiteSpace(destinationLabel)
            ? schedule.Title
            : $"{schedule.Title} · {destinationLabel}";
        var useCalmCopy = prefs.Notification.UseCalmReminderCopy;
        var prepTitle = useCalmCopy ? "이제 천천히 준비를 시작해 볼까요?" : "준비 시작 시간이에요";
        var leaveTitle = useCalmCopy ? "지금 출발하면 여유 있게 도착할 수 있어요" : "출발할 시간이에요";

        if (plan.StartPrepAt > now)
        {
            await ScheduleBrowserNotificationAsync(PrepId(schedule.Id), prepTitle, description, plan.StartPrepAt);
        }

        if (plan.LeaveAt > now)
        {
            await ScheduleBrowserNotificationAsync(LeaveId(schedule.Id), leaveTitle, description, plan.LeaveAt);
        }

        if (prefs.Notification.LeaveSoonReminderMinutes > 0)
        {
            var beforeLeave = plan.LeaveAt.AddMinutes(-prefs.Notification.LeaveSoonReminderMinutes);
            if (beforeLeave > now && beforeLeave < plan.LeaveAt)
            {
                await ScheduleBrowserNotificationAsync(
                    LeaveSoonId(schedule.Id),
                    $"출발 {prefs.Notification.LeaveSoonReminderMinutes}분 전",
                    description,
                    beforeLeave);
            }
        }
    }

    public Task CancelAsync(Guid scheduleId)
    {
        return _js.InvokeVoidAsync(
            "readierNotifications.cancelGroup",
            PrepId(scheduleId),
            LeaveId(scheduleId),
            LeaveSoonId(scheduleId)).AsTask();
    }

    private async Task<bool> EnsurePermissionAsync()
    {
        return await _js.InvokeAsync<bool>("readierNotifications.isEnabled");
    }

    private Task ScheduleBrowserNotificationAsync(int id, string title, string description, DateTime notifyTime)
        => _js.InvokeVoidAsync("readierNotifications.schedule", id, title, description, notifyTime).AsTask();

    private static int PrepId(Guid id) => HashCode.Combine(id, "prep") & 0x7FFFFFFF;

    private static int LeaveId(Guid id) => HashCode.Combine(id, "leave") & 0x7FFFFFFF;

    private static int LeaveSoonId(Guid id) => HashCode.Combine(id, "leave-soon") & 0x7FFFFFFF;
}
