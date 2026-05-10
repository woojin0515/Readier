using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Readier.Interfaces;
using Readier.Models;
using Readier.Views;

namespace Readier.ViewModels;

public partial class ScheduleListViewModel : BaseViewModel
{
    internal const string StorageKey = "readier.schedules.v1";

    private readonly IStorageService _storage;

    public ObservableCollection<Schedule> Schedules { get; } = new();

    public ScheduleListViewModel(IStorageService storage)
    {
        _storage = storage;
        Title = "내 일정";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var list = await _storage.GetAsync<List<Schedule>>(StorageKey) ?? new List<Schedule>();
            Schedules.Clear();
            foreach (var s in list.OrderBy(x => x.StartTime))
                Schedules.Add(s);
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
    private Task EditAsync(Schedule? schedule)
        => schedule is null
            ? Task.CompletedTask
            : Shell.Current.GoToAsync($"{nameof(ScheduleEditPage)}?id={schedule.Id}");

    [RelayCommand]
    private async Task DeleteAsync(Schedule? schedule)
    {
        if (schedule is null) return;

        var list = await _storage.GetAsync<List<Schedule>>(StorageKey) ?? new List<Schedule>();
        list.RemoveAll(s => s.Id == schedule.Id);
        await _storage.SetAsync(StorageKey, list);
        Schedules.Remove(schedule);
    }
}
