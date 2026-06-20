using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Readier.Helpers;
using Readier.Interfaces;
using Readier.Models;

namespace Readier.ViewModels;

public partial class BehaviorAnalysisViewModel : BaseViewModel
{
    private readonly IUserPreferencesService _preferences;
    private bool _isLoading = true;
    private CancellationTokenSource? _saveDebounce;

    public ObservableCollection<SurveyQuestionViewModel> Questions { get; } = new();

    [ObservableProperty]
    private int estimatedPrepMinutes;

    public BehaviorAnalysisViewModel(IUserPreferencesService preferences)
    {
        _preferences = preferences;
        Title = "분석";

        foreach (var question in PreparationSurvey.All)
        {
            var qvm = new SurveyQuestionViewModel(question, 0);
            qvm.PropertyChanged += OnQuestionAnswerChanged;
            Questions.Add(qvm);
        }
    }

    public async Task LoadAsync()
    {
        _isLoading = true;
        try
        {
            var prefs = await _preferences.GetAsync();
            foreach (var qvm in Questions)
            {
                qvm.SelectedIndex = prefs.PreparationProfile.Answers.TryGetValue(qvm.Key, out var idx)
                    ? idx
                    : 0;
            }
            UpdateTotal();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async void OnQuestionAnswerChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SurveyQuestionViewModel.SelectedIndex)) return;
        UpdateTotal();
        if (_isLoading) return;
        QueueSave();
    }

    private void UpdateTotal()
    {
        EstimatedPrepMinutes = PreparationSurvey.TotalMinutes(SnapshotAnswers());
    }

    public async Task SaveAsync()
    {
        var prefs = await _preferences.GetAsync();
        prefs.PreparationProfile = new PreparationProfile { Answers = SnapshotAnswers() };
        await _preferences.SaveAsync(prefs);
    }

    private void QueueSave()
    {
        _saveDebounce?.Cancel();
        _saveDebounce = new CancellationTokenSource();
        var token = _saveDebounce.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(250, token);
                if (token.IsCancellationRequested) return;
                await SaveAsync();
            }
            catch (TaskCanceledException)
            {
            }
        });
    }

    private Dictionary<string, int> SnapshotAnswers()
        => Questions.ToDictionary(q => q.Key, q => q.SelectedIndex);
}
