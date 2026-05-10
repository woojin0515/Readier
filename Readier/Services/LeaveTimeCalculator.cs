using Readier.Interfaces;
using Readier.Models;

namespace Readier.Services;

public class LeaveTimeCalculator : ILeaveTimeCalculator
{
    public LeaveTimePlan Calculate(Schedule schedule)
    {
        var leaveAt = schedule.StartTime.AddMinutes(-schedule.EstimatedTravelMinutes);
        var startPrepAt = leaveAt.AddMinutes(-schedule.EstimatedPrepMinutes);

        return new LeaveTimePlan
        {
            StartPrepAt = startPrepAt,
            LeaveAt = leaveAt
        };
    }
}
