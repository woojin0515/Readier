using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Readier.Helpers;
using Readier.Interfaces;
using Readier.Models;

namespace Readier.ViewModels;

[QueryProperty(nameof(Id), "id")]
public partial class ScheduleEditViewModel : BaseViewModel
{
    private readonly IStorageService _storage;
    private readonly ILeaveTimeCalculator _calculator;
    private readonly IScheduleNotificationService _notifications;
    private readonly ITravelTimeProvider _travelProvider;

    public IPlaceSearchService PlaceSearch { get; }

    [ObservableProperty]
    private string id = string.Empty;

    [ObservableProperty]
    private string scheduleTitle = string.Empty;

    [ObservableProperty]
    private Place? originPlace;

    [ObservableProperty]
    private Place? destinationPlace;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LeaveAt))]
    [NotifyPropertyChangedFor(nameof(StartPrepAt))]
    private DateTime startDate = DateTime.Today;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LeaveAt))]
    [NotifyPropertyChangedFor(nameof(StartPrepAt))]
    private TimeSpan startTime = DateTime.Now.TimeOfDay;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LeaveAt))]
    [NotifyPropertyChangedFor(nameof(StartPrepAt))]
    private int travelMinutes = 30;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StartPrepAt))]
    private int prepMinutes = 30;

    [ObservableProperty]
    private TransportationOption? transportationOption;

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotCalculating))]
    private bool isCalculating;

    public bool IsNotCalculating => !IsCalculating;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(HasInfo))]
    private string? statusMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(HasInfo))]
    private bool isStatusError;

    public bool HasError => !string.IsNullOrEmpty(StatusMessage) && IsStatusError;

    public bool HasInfo => !string.IsNullOrEmpty(StatusMessage) && !IsStatusError;

    public DateTime LeaveAt => CalculatePlan().LeaveAt;

    public DateTime StartPrepAt => CalculatePlan().StartPrepAt;

    public IReadOnlyList<TransportationOption> TransportationOptions { get; } = TransportationCatalog.All;

    public ScheduleEditViewModel(
        IStorageService storage,
        ILeaveTimeCalculator calculator,
        IScheduleNotificationService notifications,
        ITravelTimeProvider travelProvider,
        IPlaceSearchService placeSearch)
    {
        _storage = storage;
        _calculator = calculator;
        _notifications = notifications;
        _travelProvider = travelProvider;
        PlaceSearch = placeSearch;
        TransportationOption = TransportationCatalog.FromMode(TransportationMode.PublicTransit);
        Title = "일정 추가";
    }

    private LeaveTimePlan CalculatePlan() => _calculator.Calculate(new Schedule
    {
        StartTime = StartDate.Date + StartTime,
        EstimatedTravelMinutes = TravelMinutes,
        EstimatedPrepMinutes = PrepMinutes
    });

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
        OriginPlace = existing.Origin;
        DestinationPlace = existing.Destination;
        StartDate = existing.StartTime.Date;
        StartTime = existing.StartTime.TimeOfDay;
        TravelMinutes = existing.EstimatedTravelMinutes;
        PrepMinutes = existing.EstimatedPrepMinutes;
        TransportationOption = TransportationCatalog.FromMode(existing.Transportation);
    }

    [RelayCommand]
    private async Task CalculateTravelTimeAsync()
    {
        StatusMessage = null;

        if (OriginPlace is null || !OriginPlace.HasCoordinates)
        {
            StatusMessage = "출발지를 선택해 주세요";
            IsStatusError = true;
            return;
        }
        if (DestinationPlace is null || !DestinationPlace.HasCoordinates)
        {
            StatusMessage = "목적지를 선택해 주세요";
            IsStatusError = true;
            return;
        }

        var mode = TransportationOption?.Mode ?? TransportationMode.PublicTransit;
        IsCalculating = true;
        try
        {
            var estimate = await _travelProvider.EstimateAsync(OriginPlace, DestinationPlace, mode);
            if (estimate is null)
            {
                StatusMessage = "이동 시간을 계산하지 못했습니다. 직접 입력해 주세요.";
                IsStatusError = true;
                return;
            }

            TravelMinutes = estimate.Minutes;
            var noteText = string.IsNullOrEmpty(estimate.Note) ? string.Empty : $" ({estimate.Note})";
            StatusMessage = $"예상 {estimate.Minutes}분 · {estimate.DistanceKm:F1}km{noteText}";
            IsStatusError = false;
        }
        finally
        {
            IsCalculating = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        StatusMessage = null;

        if (string.IsNullOrWhiteSpace(ScheduleTitle))
        {
            StatusMessage = "제목을 입력해 주세요";
            IsStatusError = true;
            return;
        }
        if (TravelMinutes < 0 || PrepMinutes < 0)
        {
            StatusMessage = "이동·준비 시간은 0분 이상이어야 합니다";
            IsStatusError = true;
            return;
        }

        var list = await _storage.GetAsync<List<Schedule>>(ScheduleListViewModel.StorageKey)
                   ?? new List<Schedule>();
        var combined = StartDate.Date + StartTime;
        var transportation = TransportationOption?.Mode ?? TransportationMode.PublicTransit;
        Schedule saved;

        if (IsEditMode && Guid.TryParse(Id, out var guid))
        {
            var idx = list.FindIndex(s => s.Id == guid);
            if (idx >= 0)
            {
                list[idx].Title = ScheduleTitle.Trim();
                list[idx].Origin = OriginPlace;
                list[idx].Destination = DestinationPlace;
                list[idx].StartTime = combined;
                list[idx].EstimatedTravelMinutes = TravelMinutes;
                list[idx].EstimatedPrepMinutes = PrepMinutes;
                list[idx].Transportation = transportation;
                saved = list[idx];
            }
            else
            {
                saved = AppendNew(list, combined, transportation);
            }
        }
        else
        {
            saved = AppendNew(list, combined, transportation);
        }

        await _storage.SetAsync(ScheduleListViewModel.StorageKey, list);
        await _notifications.ScheduleAsync(saved);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private Task CancelAsync()
        => Shell.Current.GoToAsync("..");

    private Schedule AppendNew(List<Schedule> list, DateTime startDateTime, TransportationMode transportation)
    {
        var schedule = new Schedule
        {
            Title = ScheduleTitle.Trim(),
            Origin = OriginPlace,
            Destination = DestinationPlace,
            StartTime = startDateTime,
            EstimatedTravelMinutes = TravelMinutes,
            EstimatedPrepMinutes = PrepMinutes,
            Transportation = transportation
        };
        list.Add(schedule);
        return schedule;
    }
}
