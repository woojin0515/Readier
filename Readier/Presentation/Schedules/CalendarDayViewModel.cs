using CommunityToolkit.Mvvm.ComponentModel;

namespace Readier.ViewModels;

public partial class CalendarDayViewModel : ObservableObject
{
    public DateTime Date { get; }

    public bool IsCurrentMonth { get; }

    public bool HasItems { get; }

    public string DayLabel => Date.Day.ToString();

    [ObservableProperty]
    private bool isSelected;

    public CalendarDayViewModel(DateTime date, bool isCurrentMonth, bool hasItems, bool isSelected)
    {
        Date = date.Date;
        IsCurrentMonth = isCurrentMonth;
        HasItems = hasItems;
        IsSelected = isSelected;
    }
}
