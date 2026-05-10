namespace Readier.ViewModels;

public class ScheduleGroup : List<ScheduleListItemViewModel>
{
    public string Title { get; }

    public DateTime Date { get; }

    public ScheduleGroup(DateTime date, string title, IEnumerable<ScheduleListItemViewModel> items)
        : base(items)
    {
        Date = date;
        Title = title;
    }
}
