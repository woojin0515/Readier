using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Readier.Interfaces;
using Readier.Models;
using Readier.Views;

namespace Readier.ViewModels;

public partial class ScheduleListViewModel : BaseViewModel
{
    internal const string StorageKey = "readier.schedules.v1";

    private static readonly CultureInfo KoreanCulture = new("ko-KR");

    private readonly IStorageService _storage;
    private readonly IScheduleNotificationService _notifications;
    private readonly ILeaveTimeCalculator _calculator;
    private List<ScheduleListItemViewModel> _allItems = new();

    public ObservableCollection<ScheduleListItemViewModel> VisibleItems { get; } = new();

    public ObservableCollection<CalendarDayViewModel> CalendarDays { get; } = new();

    [ObservableProperty]
    private bool isAgendaMode = true;

    [ObservableProperty]
    private bool isEmpty;

    [ObservableProperty]
    private string agendaTitle = "오늘 일정";

    [ObservableProperty]
    private DateTime selectedDate = DateTime.Today;

    [ObservableProperty]
    private DateTime calendarMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    public bool IsCalendarMode => !IsAgendaMode;

    public string CalendarMonthTitle => CalendarMonth.ToString("yyyy년 M월", KoreanCulture);

    public ScheduleListViewModel(
        IStorageService storage,
        IScheduleNotificationService notifications,
        ILeaveTimeCalculator calculator)
    {
        _storage = storage;
        _notifications = notifications;
        _calculator = calculator;
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

    [RelayCommand]
    private Task AddAsync()
        => Shell.Current.GoToAsync(nameof(ScheduleEditPage));

    [RelayCommand]
    private Task EditAsync(ScheduleListItemViewModel? item)
        => item is null
            ? Task.CompletedTask
            : Shell.Current.GoToAsync($"{nameof(ScheduleEditPage)}?id={item.Id}");

    [RelayCommand]
    private async Task DeleteAsync(ScheduleListItemViewModel? item)
    {
        if (item is null) return;

        await _notifications.CancelAsync(item.Id);

        var list = await _storage.GetAsync<List<Schedule>>(StorageKey) ?? new List<Schedule>();
        list.RemoveAll(s => s.Id == item.Id);
        await _storage.SetAsync(StorageKey, list);

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
        SelectedDate = DateTime.Today;
        CalendarMonth = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
        IsAgendaMode = true;
    }

    private async Task RebuildAsync()
    {
        var list = await _storage.GetAsync<List<Schedule>>(StorageKey) ?? new List<Schedule>();
        _allItems = list
            .OrderBy(s => s.StartTime)
            .Select(s => new ScheduleListItemViewModel(s, _calculator.Calculate(s)))
            .ToList();

        RebuildVisibleItems();
        RebuildCalendarDays();
    }

    private void RebuildVisibleItems()
    {
        var filtered = _allItems
            .Where(i => i.StartTime.Date == SelectedDate.Date)
            .OrderBy(i => i.StartTime)
            .ToList();

        VisibleItems.Clear();
        foreach (var item in filtered) VisibleItems.Add(item);

        AgendaTitle = FormatAgendaTitle(SelectedDate);
        IsEmpty = VisibleItems.Count == 0;
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

    private static string FormatAgendaTitle(DateTime date)
    {
        var today = DateTime.Today;
        if (date == today) return "오늘 일정";
        if (date == today.AddDays(1)) return "내일 일정";
        if (date == today.AddDays(-1)) return "어제 일정";
        return date.ToString("M월 d일 (ddd) 일정", KoreanCulture);
    }
}
