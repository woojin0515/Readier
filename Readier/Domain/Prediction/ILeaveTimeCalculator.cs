using Readier.Models;

namespace Readier.Interfaces;

public interface ILeaveTimeCalculator
{
    LeaveTimePlan Calculate(Schedule schedule);
}
