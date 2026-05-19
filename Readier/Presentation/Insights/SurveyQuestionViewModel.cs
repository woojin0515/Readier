using CommunityToolkit.Mvvm.ComponentModel;
using Readier.Helpers;

namespace Readier.ViewModels;

public partial class SurveyQuestionViewModel : ObservableObject
{
    public string Key { get; }

    public string QuestionTitle { get; }

    public IReadOnlyList<string> OptionLabels { get; }

    [ObservableProperty]
    private int selectedIndex;

    public SurveyQuestionViewModel(SurveyQuestion question, int initialIndex)
    {
        Key = question.Key;
        QuestionTitle = question.Title;
        OptionLabels = question.Options.Select(o => o.Label).ToList();
        SelectedIndex = initialIndex >= 0 && initialIndex < OptionLabels.Count ? initialIndex : 0;
    }
}
