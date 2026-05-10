using System.Collections.ObjectModel;
using Readier.Interfaces;
using Readier.Models;

namespace Readier.Controls;

public partial class PlacePickerControl : ContentView
{
    public static readonly BindableProperty SearchServiceProperty = BindableProperty.Create(
        nameof(SearchService),
        typeof(IPlaceSearchService),
        typeof(PlacePickerControl));

    public static readonly BindableProperty SelectedPlaceProperty = BindableProperty.Create(
        nameof(SelectedPlace),
        typeof(Place),
        typeof(PlacePickerControl),
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnSelectedPlaceChanged);

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder),
        typeof(string),
        typeof(PlacePickerControl),
        string.Empty);

    public static readonly BindableProperty QueryTextProperty = BindableProperty.Create(
        nameof(QueryText),
        typeof(string),
        typeof(PlacePickerControl),
        string.Empty);

    public static readonly BindableProperty IsSearchingProperty = BindableProperty.Create(
        nameof(IsSearching),
        typeof(bool),
        typeof(PlacePickerControl),
        false);

    public static readonly BindableProperty HasResultsProperty = BindableProperty.Create(
        nameof(HasResults),
        typeof(bool),
        typeof(PlacePickerControl),
        false);

    public ObservableCollection<PlaceSearchResult> Results { get; } = new();

    private CancellationTokenSource? _debounceCts;
    private bool _suppressTextChanged;

    public PlacePickerControl()
    {
        InitializeComponent();
    }

    public IPlaceSearchService? SearchService
    {
        get => (IPlaceSearchService?)GetValue(SearchServiceProperty);
        set => SetValue(SearchServiceProperty, value);
    }

    public Place? SelectedPlace
    {
        get => (Place?)GetValue(SelectedPlaceProperty);
        set => SetValue(SelectedPlaceProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string QueryText
    {
        get => (string)GetValue(QueryTextProperty);
        set => SetValue(QueryTextProperty, value);
    }

    public bool IsSearching
    {
        get => (bool)GetValue(IsSearchingProperty);
        set => SetValue(IsSearchingProperty, value);
    }

    public bool HasResults
    {
        get => (bool)GetValue(HasResultsProperty);
        set => SetValue(HasResultsProperty, value);
    }

    private static void OnSelectedPlaceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not PlacePickerControl control) return;
        var place = newValue as Place;
        control.UpdateEntryFromPlace(place);
    }

    private void UpdateEntryFromPlace(Place? place)
    {
        _suppressTextChanged = true;
        try
        {
            QueryText = place?.DisplayLine ?? string.Empty;
            Results.Clear();
            HasResults = false;
        }
        finally
        {
            _suppressTextChanged = false;
        }
    }

    private async void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_suppressTextChanged) return;

        var query = e.NewTextValue ?? string.Empty;

        if (string.IsNullOrWhiteSpace(query))
        {
            _debounceCts?.Cancel();
            SelectedPlace = null;
            Results.Clear();
            HasResults = false;
            IsSearching = false;
            return;
        }

        if (SelectedPlace is not null && SelectedPlace.DisplayLine != query)
        {
            SelectedPlace = null;
        }

        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        try
        {
            await Task.Delay(300, token);
            if (token.IsCancellationRequested) return;
            if (SearchService is null) return;

            IsSearching = true;
            var results = await SearchService.SearchAsync(query, token);
            if (token.IsCancellationRequested) return;

            Results.Clear();
            foreach (var r in results) Results.Add(r);
            HasResults = Results.Count > 0;
        }
        catch (TaskCanceledException)
        {
            // debounced — ignore
        }
        catch
        {
            // swallow transient errors quietly; UI remains usable
        }
        finally
        {
            if (!token.IsCancellationRequested) IsSearching = false;
        }
    }

    private async void OnResultTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not PlaceSearchResult result) return;

        var address = !string.IsNullOrWhiteSpace(result.RoadAddress) ? result.RoadAddress : result.Address;

        if (result.HasCoordinates)
        {
            SelectedPlace = new Place
            {
                Name = result.Name,
                Address = address,
                Latitude = result.Latitude,
                Longitude = result.Longitude
            };
            return;
        }

        if (SearchService is null) return;
        if (string.IsNullOrWhiteSpace(address)) return;

        try
        {
            IsSearching = true;
            var place = await SearchService.GeocodeAsync(address, result.Name);
            if (place is null) return;
            SelectedPlace = place;
        }
        finally
        {
            IsSearching = false;
        }
    }
}
