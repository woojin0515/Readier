using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Readier.Helpers;
using Readier.Interfaces;
using Readier.Models;

namespace Readier.ViewModels;

public partial class ScheduleEditViewModel : BaseViewModel, IDisposable
{
    private readonly IStorageService _storage;
    private readonly ILeaveTimeCalculator _calculator;
    private readonly IScheduleNotificationService _notifications;
    private readonly ITravelTimeProvider _travelProvider;
    private readonly IUserPreferencesService _preferences;
    private readonly NavigationManager _navigation;
    private bool _isInitializing;
    private bool _isAutoFilledPrep = true;

    public IPlaceSearchService PlaceSearch { get; }

    [ObservableProperty]
    private IReadOnlyList<Place> recentPlaces = Array.Empty<Place>();

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
    private TimeOnly startTime = TimeOnly.FromDateTime(DateTime.Now);

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

    public DateTime DisplayLeaveAt => LeaveAt <= DateTime.Now ? DateTime.Now : LeaveAt;

    public DateTime DisplayStartPrepAt => StartPrepAt <= DateTime.Now ? DateTime.Now : StartPrepAt;

    public bool IsLeaveLate => LeaveAt <= DateTime.Now;

    public bool IsStartPrepLate => StartPrepAt <= DateTime.Now;

    public IReadOnlyList<TransportationOption> TransportationOptions { get; } = TransportationCatalog.All;

    public ScheduleEditViewModel(
        IStorageService storage,
        ILeaveTimeCalculator calculator,
        IScheduleNotificationService notifications,
        ITravelTimeProvider travelProvider,
        IPlaceSearchService placeSearch,
        IUserPreferencesService preferences,
        NavigationManager navigation)
    {
        _storage = storage;
        _calculator = calculator;
        _notifications = notifications;
        _travelProvider = travelProvider;
        _preferences = preferences;
        _navigation = navigation;
        PlaceSearch = placeSearch;
        TransportationOption = TransportationCatalog.FromMode(TransportationMode.PublicTransit);
        Title = "일정 추가";
        _preferences.PreferencesChanged += OnPreferencesChanged;
    }

    partial void OnPrepMinutesChanged(int value)
    {
        if (_isInitializing) return;
        _isAutoFilledPrep = false;
    }

    private LeaveTimePlan CalculatePlan() => _calculator.Calculate(new Schedule
    {
        StartTime = StartDate.Date + StartTime.ToTimeSpan(),
        EstimatedTravelMinutes = TravelMinutes,
        EstimatedPrepMinutes = PrepMinutes
    });

    public async Task LoadAsync(string? idValue)
    {
        _isInitializing = true;
        var prefs = await _preferences.GetAsync();
        RecentPlaces = prefs.RecentPlaces.ToList();
        Id = idValue ?? string.Empty;
        if (!Guid.TryParse(idValue, out var guid))
        {
            IsEditMode = false;
            Title = "일정 추가";
            ScheduleTitle = string.Empty;
            OriginPlace = RecentPlaces.FirstOrDefault();
            DestinationPlace = null;
            StartDate = DateTime.Today;
            StartTime = TimeOnly.FromDateTime(DateTime.Now);
            TravelMinutes = 30;
            PrepMinutes = GetDefaultPrepMinutes(prefs);
            _isAutoFilledPrep = true;
            TransportationOption = TransportationCatalog.FromMode(prefs.PreferredTransportation);
            StatusMessage = null;
            IsStatusError = false;
            _isInitializing = false;
            return;
        }

        var list = await _storage.GetAsync<List<Schedule>>(ScheduleListViewModel.StorageKey)
                   ?? new List<Schedule>();
        var existing = list.FirstOrDefault(s => s.Id == guid);
        if (existing is null)
        {
            _isInitializing = false;
            return;
        }

        IsEditMode = true;
        Title = "일정 수정";
        ScheduleTitle = existing.Title;
        OriginPlace = existing.Origin;
        DestinationPlace = existing.Destination;
        StartDate = existing.StartTime.Date;
        StartTime = TimeOnly.FromDateTime(existing.StartTime);
        TravelMinutes = existing.EstimatedTravelMinutes;
        PrepMinutes = existing.EstimatedPrepMinutes;
        TransportationOption = TransportationCatalog.FromMode(existing.Transportation);
        StatusMessage = null;
        IsStatusError = false;
        _isAutoFilledPrep = false;
        _isInitializing = false;
    }

    private static int GetDefaultPrepMinutes(UserPreferences prefs)
    {
        var minutes = PreparationSurvey.TotalMinutes(prefs.PreparationProfile.Answers);
        return minutes > 0 ? minutes : 30;
    }

    private async void OnPreferencesChanged(object? sender, EventArgs e)
    {
        if (IsEditMode || !_isAutoFilledPrep || _isInitializing)
            return;

        PrepMinutes = GetDefaultPrepMinutes(await _preferences.GetAsync());
    }

    public void Dispose()
    {
        _preferences.PreferencesChanged -= OnPreferencesChanged;
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
        var combined = StartDate.Date + StartTime.ToTimeSpan();
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
        await SaveRecentPreferencesAsync(transportation);
        await _notifications.ScheduleAsync(saved);
        _navigation.NavigateTo("/");
    }

    [RelayCommand]
    private Task CancelAsync()
    {
        _navigation.NavigateTo("/");
        return Task.CompletedTask;
    }

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

    [RelayCommand]
    private void UseRecentOrigin(Place? place)
    {
        if (place is null) return;
        OriginPlace = place;
    }

    [RelayCommand]
    private void UseRecentDestination(Place? place)
    {
        if (place is null) return;
        DestinationPlace = place;
    }

    private async Task SaveRecentPreferencesAsync(TransportationMode transportation)
    {
        var prefs = await _preferences.GetAsync();
        prefs.PreferredTransportation = transportation;
        prefs.RecentPlaces = MergeRecentPlaces(prefs.RecentPlaces, OriginPlace, DestinationPlace);
        await _preferences.SaveAsync(prefs);
        RecentPlaces = prefs.RecentPlaces.ToList();
    }

    private static List<Place> MergeRecentPlaces(IEnumerable<Place> current, Place? origin, Place? destination)
    {
        var merged = current.Where(IsUsablePlace).ToList();
        UpsertRecent(merged, origin);
        UpsertRecent(merged, destination);
        return merged.Take(5).ToList();
    }

    private static bool IsUsablePlace(Place place)
        => !string.IsNullOrWhiteSpace(place.DisplayLine) || !string.IsNullOrWhiteSpace(place.Address);

    private static void UpsertRecent(List<Place> places, Place? candidate)
    {
        if (candidate is null || !IsUsablePlace(candidate))
            return;

        places.RemoveAll(existing => AreSamePlace(existing, candidate));
        places.Insert(0, ClonePlace(candidate));
    }

    private static bool AreSamePlace(Place left, Place right)
    {
        if (left.HasCoordinates && right.HasCoordinates)
        {
            return Math.Abs(left.Latitude - right.Latitude) < 0.000001
                   && Math.Abs(left.Longitude - right.Longitude) < 0.000001;
        }

        return string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase)
               && string.Equals(left.Address, right.Address, StringComparison.OrdinalIgnoreCase);
    }

    private static Place ClonePlace(Place place)
        => new()
        {
            Name = place.Name,
            Address = place.Address,
            Latitude = place.Latitude,
            Longitude = place.Longitude
        };
}
