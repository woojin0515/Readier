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

    public ObservableCollection<ScheduleGroup> Groups { get; } = new();

    [ObservableProperty]
    private bool isEmpty;

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

    private async Task RebuildAsync()
    {
        var list = await _storage.GetAsync<List<Schedule>>(StorageKey) ?? new List<Schedule>();
        var items = list
            .OrderBy(s => s.StartTime)
            .Select(s => new ScheduleListItemViewModel(s, _calculator.Calculate(s)))
            .ToList();

        var grouped = items
            .GroupBy(i => i.StartTime.Date)
            .OrderBy(g => g.Key)
            .Select(g => new ScheduleGroup(g.Key, FormatGroupTitle(g.Key), g))
            .ToList();

        Groups.Clear();
        foreach (var group in grouped) Groups.Add(group);

        IsEmpty = Groups.Count == 0;
    }

    private static string FormatGroupTitle(DateTime date)
    {
        var today = DateTime.Today;
        if (date == today) return "오늘";
        if (date == today.AddDays(1)) return "내일";
        if (date == today.AddDays(-1)) return "어제";
        return date.ToString("M월 d일 (ddd)", KoreanCulture);
    }
}
