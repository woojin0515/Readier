using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Readier.Helpers;
using Readier.Interfaces;
using Readier.Models;

namespace Readier.ViewModels;

public partial class ScheduleListViewModel : BaseViewModel
{
    private static readonly CultureInfo KoreanCulture = new("ko-KR");

    private readonly IPlanRepository _plans;
    private readonly IScheduleNotificationService _notifications;
    private readonly ILeaveTimeCalculator _calculator;
    private readonly NavigationManager _navigation;
    private List<ScheduleListItemViewModel> _allItems = new();

    public ObservableCollection<ScheduleListItemViewModel> VisibleItems { get; } = new();

    public ObservableCollection<CalendarDayViewModel> CalendarDays { get; } = new();

    [ObservableProperty]
    private bool isAgendaMode = true;

    [ObservableProperty]
    private bool isEmpty;

    [ObservableProperty]
    private bool showLateOnly;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNextUpcoming))]
    [NotifyPropertyChangedFor(nameof(NextUpcomingSummary))]
    private ScheduleListItemViewModel? nextUpcomingItem;

    [ObservableProperty]
    private string agendaTitle = "오늘 일정";

    [ObservableProperty]
    private DateTime selectedDate = AppClock.Today;

    [ObservableProperty]
    private DateTime calendarMonth = new(AppClock.Today.Year, AppClock.Today.Month, 1);

    [ObservableProperty]
    private bool isThisWeekMode;

    public ObservableCollection<DateTime> ThisWeekDates { get; } = new();

    public bool IsCalendarMode => !IsAgendaMode;

    public bool HasNextUpcoming => NextUpcomingItem is not null;

    public string NextUpcomingSummary => BuildNextUpcomingSummary(NextUpcomingItem);

    public string CalendarMonthTitle => CalendarMonth.ToString("yyyy년 M월", KoreanCulture);

    public ScheduleListViewModel(
        IPlanRepository plans,
        IScheduleNotificationService notifications,
        ILeaveTimeCalculator calculator,
        NavigationManager navigation)
    {
        _plans = plans;
        _notifications = notifications;
        _calculator = calculator;
        _navigation = navigation;
        Title = "일정";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            await RebuildAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnIsAgendaModeChanged(bool value)
    {
        OnPropertyChanged(nameof(IsCalendarMode));
        RebuildVisibleItems();
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        RebuildVisibleItems();
        RebuildCalendarDays();
    }

    partial void OnCalendarMonthChanged(DateTime value)
    {
        OnPropertyChanged(nameof(CalendarMonthTitle));
        RebuildCalendarDays();
    }

    partial void OnShowLateOnlyChanged(bool value)
    {
        RebuildVisibleItems();
    }

    [RelayCommand]
    private Task AddAsync()
    {
        _navigation.NavigateTo("/schedules/edit");
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task EditAsync(ScheduleListItemViewModel? item)
    {
        if (item is null) return Task.CompletedTask;
        _navigation.NavigateTo($"/schedules/edit/{item.Id}");
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task DeleteAsync(ScheduleListItemViewModel? item)
    {
        if (item is null) return;

        await _notifications.CancelAsync(item.Id);
        await _plans.DeleteAsync(item.Id);

        await RebuildAsync();
    }

    [RelayCommand]
    private void ShowAgenda()
    {
        IsAgendaMode = true;
    }

    [RelayCommand]
    private void ShowCalendar()
    {
        IsAgendaMode = false;
    }

    [RelayCommand]
    private void PreviousMonth()
    {
        CalendarMonth = CalendarMonth.AddMonths(-1);
    }

    [RelayCommand]
    private void NextMonth()
    {
        CalendarMonth = CalendarMonth.AddMonths(1);
    }

    [RelayCommand]
    private void SelectDate(DateTime date)
    {
        SelectedDate = date.Date;
        CalendarMonth = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
    }

    [RelayCommand]
    private void GoToToday()
    {
        SelectedDate = AppClock.Today;
        CalendarMonth = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
        IsAgendaMode = true;
        IsThisWeekMode = false;
    }

    [RelayCommand]
    private void GoToTomorrow()
    {
        SelectedDate = AppClock.Today.AddDays(1);
        CalendarMonth = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
        IsAgendaMode = true;
        IsThisWeekMode = false;
    }

    [RelayCommand]
    private void GoToThisWeek()
    {
        IsAgendaMode = true;
        IsThisWeekMode = true;
        RebuildThisWeekDates();
        if (!IsDateInCurrentWeek(SelectedDate))
            SelectedDate = AppClock.Today;
    }

    [RelayCommand]
    private void SelectThisWeekDate(DateTime date)
    {
        IsAgendaMode = true;
        IsThisWeekMode = true;
        SelectedDate = date.Date;
    }

    private async Task RebuildAsync()
    {
        var list = (await _plans.ListAsync()).ToList();
        _allItems = list
            .OrderBy(s => s.StartTime)
            .Select(s => new ScheduleListItemViewModel(s, _calculator.Calculate(s)))
            .ToList();

        RebuildVisibleItems();
        RebuildCalendarDays();
        RebuildThisWeekDates();
        NextUpcomingItem = _allItems.FirstOrDefault(i => i.StartTime >= AppClock.Now);

        foreach (var item in list)
        {
            await _notifications.ScheduleAsync(item);
        }
    }

    private void RebuildVisibleItems()
    {
        var filtered = _allItems
            .Where(i => i.StartTime.Date == SelectedDate.Date)
            .Where(i => !ShowLateOnly || i.IsLeaveLate || i.IsStartPrepLate)
            .OrderBy(i => i.StartTime)
            .ToList();

        VisibleItems.Clear();
        foreach (var item in filtered) VisibleItems.Add(item);

        AgendaTitle = FormatAgendaTitle(SelectedDate, ShowLateOnly);
        IsEmpty = VisibleItems.Count == 0;
    }

    public void RefreshNowSensitiveState()
    {
        RebuildVisibleItems();
        NextUpcomingItem = _allItems.FirstOrDefault(i => i.StartTime >= AppClock.Now);
        OnPropertyChanged(nameof(NextUpcomingSummary));
    }

    private void RebuildCalendarDays()
    {
        CalendarDays.Clear();

        var firstDay = new DateTime(CalendarMonth.Year, CalendarMonth.Month, 1);
        var startOffset = (int)firstDay.DayOfWeek;
        var startDate = firstDay.AddDays(-startOffset);

        for (var i = 0; i < 42; i++)
        {
            var date = startDate.AddDays(i).Date;
            var isCurrentMonth = date.Month == CalendarMonth.Month && date.Year == CalendarMonth.Year;
            var hasItems = _allItems.Any(x => x.StartTime.Date == date);
            CalendarDays.Add(new CalendarDayViewModel(date, isCurrentMonth, hasItems, date == SelectedDate.Date));
        }
    }

    private static string FormatAgendaTitle(DateTime date, bool lateOnly)
    {
        var today = AppClock.Today;
        var suffix = lateOnly ? " · 늦은 일정만" : string.Empty;
        if (date == today) return $"오늘 일정{suffix}";
        if (date == today.AddDays(1)) return $"내일 일정{suffix}";
        if (date == today.AddDays(-1)) return $"어제 일정{suffix}";
        return $"{date.ToString("M월 d일 (ddd) 일정", KoreanCulture)}{suffix}";
    }

    private static string BuildNextUpcomingSummary(ScheduleListItemViewModel? item)
    {
        if (item is null)
            return string.Empty;

        var now = AppClock.Now;
        var target = item.StartPrepAt > now
            ? item.StartPrepAt
            : item.LeaveAt > now
                ? item.LeaveAt
                : item.StartTime;

        var remaining = target - now;
        if (remaining < TimeSpan.Zero)
            remaining = TimeSpan.Zero;

        var countdown = remaining.TotalHours >= 1
            ? $"{(int)remaining.TotalHours}시간 {remaining.Minutes}분"
            : $"{Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes))}분";

        if (item.StartPrepAt > now)
            return $"준비 시작까지 {countdown}";

        if (item.LeaveAt > now)
            return $"출발까지 {countdown}";

        return "이미 지난 일정이에요";
    }

    private void RebuildThisWeekDates()
    {
        ThisWeekDates.Clear();
        var start = AppClock.Today.AddDays(-(int)(AppClock.Today.DayOfWeek == DayOfWeek.Sunday ? 6 : AppClock.Today.DayOfWeek - DayOfWeek.Monday));
        for (var i = 0; i < 7; i++)
        {
            ThisWeekDates.Add(start.AddDays(i).Date);
        }
    }

    private static bool IsDateInCurrentWeek(DateTime date)
    {
        var today = AppClock.Today;
        var start = today.AddDays(-(int)(today.DayOfWeek == DayOfWeek.Sunday ? 6 : today.DayOfWeek - DayOfWeek.Monday));
        var end = start.AddDays(6);
        return date.Date >= start && date.Date <= end;
    }
}
