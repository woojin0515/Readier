using Readier.Models;

namespace Readier.ViewModels;

public class ScheduleListItemViewModel
{
    public Schedule Schedule { get; }

    public DateTime LeaveAt { get; }

    public DateTime StartPrepAt { get; }

    public ScheduleListItemViewModel(Schedule schedule, LeaveTimePlan plan)
    {
        Schedule = schedule;
        LeaveAt = plan.LeaveAt;
        StartPrepAt = plan.StartPrepAt;
    }

    public Guid Id => Schedule.Id;
    public string Title => Schedule.Title;
    public DateTime StartTime => Schedule.StartTime;

    public string DestinationLabel => Schedule.Destination?.DisplayLine ?? string.Empty;

    public bool HasDestination => !string.IsNullOrWhiteSpace(DestinationLabel);
}
