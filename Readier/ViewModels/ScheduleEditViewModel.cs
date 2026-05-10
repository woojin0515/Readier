using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Readier.Interfaces;
using Readier.Models;

namespace Readier.ViewModels;

[QueryProperty(nameof(Id), "id")]
public partial class ScheduleEditViewModel : BaseViewModel
{
    private readonly IStorageService _storage;

    [ObservableProperty]
    private string id = string.Empty;

    [ObservableProperty]
    private string scheduleTitle = string.Empty;

    [ObservableProperty]
    private string location = string.Empty;

    [ObservableProperty]
    private DateTime startDate = DateTime.Today;

    [ObservableProperty]
    private TimeSpan startTime = DateTime.Now.TimeOfDay;

    [ObservableProperty]
    private int travelMinutes = 30;

    [ObservableProperty]
    private int prepMinutes = 30;

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public ScheduleEditViewModel(IStorageService storage)
    {
        _storage = storage;
        Title = "일정 추가";
    }

    partial void OnIdChanged(string value)
    {
        _ = LoadIfEditingAsync(value);
    }

    private async Task LoadIfEditingAsync(string idValue)
    {
        if (!Guid.TryParse(idValue, out var guid)) return;

        var list = await _storage.GetAsync<List<Schedule>>(ScheduleListViewModel.StorageKey)
                   ?? new List<Schedule>();
        var existing = list.FirstOrDefault(s => s.Id == guid);
        if (existing is null) return;

        IsEditMode = true;
        Title = "일정 수정";
        ScheduleTitle = existing.Title;
        Location = existing.Location;
        StartDate = existing.StartTime.Date;
        StartTime = existing.StartTime.TimeOfDay;
        TravelMinutes = existing.EstimatedTravelMinutes;
        PrepMinutes = existing.EstimatedPrepMinutes;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(ScheduleTitle))
        {
            ErrorMessage = "제목을 입력해 주세요";
            return;
        }
        if (TravelMinutes < 0 || PrepMinutes < 0)
        {
            ErrorMessage = "이동·준비 시간은 0분 이상이어야 합니다";
            return;
        }

        var list = await _storage.GetAsync<List<Schedule>>(ScheduleListViewModel.StorageKey)
                   ?? new List<Schedule>();
        var combined = StartDate.Date + StartTime;

        if (IsEditMode && Guid.TryParse(Id, out var guid))
        {
            var idx = list.FindIndex(s => s.Id == guid);
            if (idx >= 0)
            {
                list[idx].Title = ScheduleTitle.Trim();
                list[idx].Location = Location.Trim();
                list[idx].StartTime = combined;
                list[idx].EstimatedTravelMinutes = TravelMinutes;
                list[idx].EstimatedPrepMinutes = PrepMinutes;
            }
        }
        else
        {
            list.Add(new Schedule
            {
                Title = ScheduleTitle.Trim(),
                Location = Location.Trim(),
                StartTime = combined,
                EstimatedTravelMinutes = TravelMinutes,
                EstimatedPrepMinutes = PrepMinutes
            });
        }

        await _storage.SetAsync(ScheduleListViewModel.StorageKey, list);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private Task CancelAsync()
        => Shell.Current.GoToAsync("..");
}
